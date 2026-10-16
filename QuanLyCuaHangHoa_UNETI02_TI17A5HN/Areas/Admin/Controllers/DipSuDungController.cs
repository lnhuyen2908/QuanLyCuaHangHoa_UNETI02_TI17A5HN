// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Controller Quản lý Dịp sử dụng (CRUD, kiểm tra trùng tên, ngừng hoạt động thì không dùng cho mẫu mới, hiển thị dữ liệu từ DB).

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class DipSuDungController : Controller
    {
        private readonly AppDbContext _context;

        public DipSuDungController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // GET: /Admin/DipSuDung/Index (Danh sách, tìm kiếm, lọc, đếm LINQ)
        // ================================================================
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.DipSuDungs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(d => d.TenDip.Contains(tuKhoa)
                                      || (d.MoTa != null && d.MoTa.Contains(tuKhoa)));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(d => d.TrangThai == trangThai.Value);
            }

            query = query.OrderByDescending(d => d.MaDip);

            // Số mẫu hoa trực thuộc đếm bằng LINQ
            var projectedQuery = query.Select(d => new DipSuDungItemViewModel
            {
                MaDip = d.MaDip,
                TenDip = d.TenDip,
                MoTa = d.MoTa,
                TrangThai = d.TrangThai,
                SoMauHoa = d.MauSanPhams.Count() // LINQ Count
            });

            var danhSach = await PagedList<DipSuDungItemViewModel>.CreateAsync(projectedQuery, page, pageSize);

            // Thống kê toàn bộ dữ liệu dịp sử dụng bằng LINQ
            var vm = new DipSuDungIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                TongSoDip = await _context.DipSuDungs.CountAsync(),
                TongDangHoatDong = await _context.DipSuDungs.CountAsync(d => d.TrangThai),
                TongNgungHoatDong = await _context.DipSuDungs.CountAsync(d => !d.TrangThai),
                TongMauHoaLienKet = await _context.MauSanPhams.CountAsync()
            };

            return View(vm);
        }

        // ================================================================
        // POST: /Admin/DipSuDung/Create (Thêm mới dịp sử dụng + Kiểm tra trùng tên)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tenDip, string? moTa, bool trangThai = true)
        {
            if (string.IsNullOrWhiteSpace(tenDip))
            {
                TempData["Loi"] = "Tên dịp sử dụng không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            var tenChuanHoa = tenDip.Trim();

            // Kiểm tra trùng tên dịp sử dụng (không phân biệt chữ hoa thường)
            if (await _context.DipSuDungs.AnyAsync(d => d.TenDip.ToLower() == tenChuanHoa.ToLower()))
            {
                TempData["Loi"] = $"Tên dịp sử dụng '{tenChuanHoa}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            var dip = new DipSuDung
            {
                TenDip = tenChuanHoa,
                MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim(),
                TrangThai = trangThai
            };

            _context.DipSuDungs.Add(dip);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Thêm dịp sử dụng '{dip.TenDip}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/DipSuDung/Edit (Cập nhật dịp sử dụng + Kiểm tra trùng tên)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maDip, string tenDip, string? moTa, bool trangThai)
        {
            if (string.IsNullOrWhiteSpace(tenDip))
            {
                TempData["Loi"] = "Tên dịp sử dụng không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            var dip = await _context.DipSuDungs.FindAsync(maDip);
            if (dip == null)
            {
                TempData["Loi"] = "Không tìm thấy dịp sử dụng cần sửa!";
                return RedirectToAction(nameof(Index));
            }

            var tenChuanHoa = tenDip.Trim();

            // Kiểm tra trùng tên với các dịp khác
            if (await _context.DipSuDungs.AnyAsync(d => d.TenDip.ToLower() == tenChuanHoa.ToLower() && d.MaDip != maDip))
            {
                TempData["Loi"] = $"Tên dịp '{tenChuanHoa}' đã trùng với dịp sử dụng khác trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            dip.TenDip = tenChuanHoa;
            dip.MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
            dip.TrangThai = trangThai;

            await _context.SaveChangesAsync();

            if (!trangThai)
            {
                TempData["ThanhCong"] = $"Cập nhật dịp '{dip.TenDip}' thành công (Trạng thái: Ngừng hoạt động - không dùng cho mẫu sản phẩm mới)!";
            }
            else
            {
                TempData["ThanhCong"] = $"Cập nhật dịp sử dụng '{dip.TenDip}' thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/DipSuDung/ToggleTrangThai (Kích hoạt / Ngừng hoạt động)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var dip = await _context.DipSuDungs.FindAsync(id);
            if (dip == null)
            {
                TempData["Loi"] = "Không tìm thấy dịp sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            dip.TrangThai = !dip.TrangThai;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = dip.TrangThai
                ? $"Dịp '{dip.TenDip}' đã được kích hoạt (có thể dùng cho mẫu sản phẩm mới)."
                : $"Dịp '{dip.TenDip}' đã chuyển sang ngừng hoạt động (không dùng cho mẫu mới).";

            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/DipSuDung/Delete (Ngừng hoạt động thì không dùng cho mẫu mới, không xóa vật lý nếu đã có mẫu hoa dùng)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dip = await _context.DipSuDungs.FindAsync(id);
            if (dip == null)
            {
                TempData["Loi"] = "Dịp sử dụng không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // KIỂM TRA RÀNG BUỘC: Nếu đã có mẫu sản phẩm liên kết thì không xóa vật lý
            var coMauDung = await _context.MauSanPhams.AnyAsync(m => m.MaDip == id);
            if (coMauDung)
            {
                // Tự động chuyển sang ngừng hoạt động (soft-deactivate) để không dùng cho mẫu mới
                dip.TrangThai = false;
                await _context.SaveChangesAsync();

                TempData["Loi"] = $"Dịp sử dụng '{dip.TenDip}' đang có các mẫu sản phẩm liên kết, không thể xóa vĩnh viễn! Hệ thống đã tự động chuyển trạng thái sang 'Ngừng hoạt động' (sẽ không thể dùng cho mẫu sản phẩm mới).";
                return RedirectToAction(nameof(Index));
            }

            // Nếu chưa có bất kỳ mẫu hoa nào liên kết -> Cho phép xóa vật lý an toàn
            _context.DipSuDungs.Remove(dip);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xóa vĩnh viễn dịp sử dụng '{dip.TenDip}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // GET: /Admin/DipSuDung/KiemTraTenDip (Kiểm tra trùng tên Realtime AJAX)
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> KiemTraTenDip(string tenDip, int? maDip)
        {
            if (string.IsNullOrWhiteSpace(tenDip))
            {
                return Json(new { hopLe = false, thongBao = "Tên dịp sử dụng không được để trống" });
            }

            var query = _context.DipSuDungs.AsQueryable();
            if (maDip.HasValue)
            {
                query = query.Where(d => d.MaDip != maDip.Value);
            }

            var daTonTai = await query.AnyAsync(d => d.TenDip.ToLower() == tenDip.Trim().ToLower());

            return Json(new
            {
                hopLe = !daTonTai,
                thongBao = daTonTai ? "Tên dịp sử dụng này đã tồn tại!" : "Tên dịp sử dụng hợp lệ."
            });
        }

        // ================================================================
        // GET: /Admin/DipSuDung/GetDanhSachHoatDong (API lấy danh sách dịp đang hoạt động để chọn cho mẫu hoa mới)
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> GetDanhSachHoatDong()
        {
            // Yêu cầu: Ngừng hoạt động thì không dùng cho mẫu mới -> chỉ lấy TrangThai == true
            var danhSach = await _context.DipSuDungs
                .AsNoTracking()
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDip)
                .Select(d => new
                {
                    d.MaDip,
                    d.TenDip
                })
                .ToListAsync();

            return Json(danhSach);
        }
    }
}
