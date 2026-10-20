// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Controller Quản lý Loại hoa (CRUD, kiểm tra trùng tên loại hoa, không xóa vật lý nếu đã có hoa tươi sử dụng, đếm số giống hoa bằng LINQ).

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
    public class LoaiHoaController : Controller
    {
        private readonly AppDbContext _context;

        public LoaiHoaController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // GET: /Admin/LoaiHoa/Index (Danh sách, tìm kiếm, lọc, đếm LINQ)
        // ================================================================
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.LoaiHoas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(l => l.TenLoaiHoa.Contains(tuKhoa)
                                      || (l.MoTa != null && l.MoTa.Contains(tuKhoa)));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(l => l.TrangThai == trangThai.Value);
            }

            query = query.OrderByDescending(l => l.MaLoaiHoa);

            // Số giống hoa trực thuộc đếm bằng LINQ
            var projectedQuery = query.Select(l => new LoaiHoaItemViewModel
            {
                MaLoaiHoa = l.MaLoaiHoa,
                TenLoaiHoa = l.TenLoaiHoa,
                MoTa = l.MoTa,
                TrangThai = l.TrangThai,
                SoGiongHoa = l.HoaTuois.Count() // LINQ Count
            });

            var danhSach = await PagedList<LoaiHoaItemViewModel>.CreateAsync(projectedQuery, page, pageSize);

            // Thống kê toàn bộ dữ liệu bằng LINQ
            var vm = new LoaiHoaIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                TongLoaiHoa = await _context.LoaiHoas.CountAsync(),
                TongDangKinhDoanh = await _context.LoaiHoas.CountAsync(l => l.TrangThai),
                TongTamNgung = await _context.LoaiHoas.CountAsync(l => !l.TrangThai),
                TongSoGiongHoa = await _context.HoaTuois.CountAsync()
            };

            return View(vm);
        }

        // ================================================================
        // POST: /Admin/LoaiHoa/Create (Thêm mới loại hoa + Kiểm tra trùng tên)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tenLoaiHoa, string? moTa, bool trangThai = true)
        {
            if (string.IsNullOrWhiteSpace(tenLoaiHoa))
            {
                TempData["Loi"] = "Tên loài hoa không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            var tenChuanHoa = tenLoaiHoa.Trim();

            // Kiểm tra trùng tên loại hoa (không phân biệt chữ hoa thường)
            if (await _context.LoaiHoas.AnyAsync(l => l.TenLoaiHoa.ToLower() == tenChuanHoa.ToLower()))
            {
                TempData["Loi"] = $"Tên loại hoa '{tenChuanHoa}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            var loaiHoa = new LoaiHoa
            {
                TenLoaiHoa = tenChuanHoa,
                MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim(),
                TrangThai = trangThai
            };

            _context.LoaiHoas.Add(loaiHoa);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Thêm loại hoa '{loaiHoa.TenLoaiHoa}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/LoaiHoa/Edit (Cập nhật loại hoa + Kiểm tra trùng tên)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int maLoaiHoa, string tenLoaiHoa, string? moTa, bool trangThai)
        {
            if (string.IsNullOrWhiteSpace(tenLoaiHoa))
            {
                TempData["Loi"] = "Tên loại hoa không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            var loaiHoa = await _context.LoaiHoas.FindAsync(maLoaiHoa);
            if (loaiHoa == null)
            {
                TempData["Loi"] = "Không tìm thấy loại hoa cần sửa!";
                return RedirectToAction(nameof(Index));
            }

            var tenChuanHoa = tenLoaiHoa.Trim();

            // Kiểm tra trùng tên với các loại hoa khác
            if (await _context.LoaiHoas.AnyAsync(l => l.TenLoaiHoa.ToLower() == tenChuanHoa.ToLower() && l.MaLoaiHoa != maLoaiHoa))
            {
                TempData["Loi"] = $"Tên loại hoa '{tenChuanHoa}' đã trùng với loại hoa khác trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            loaiHoa.TenLoaiHoa = tenChuanHoa;
            loaiHoa.MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
            loaiHoa.TrangThai = trangThai;

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Cập nhật loại hoa '{loaiHoa.TenLoaiHoa}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/LoaiHoa/Delete (Không xóa vật lý nếu đã có hoa dùng)
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var loaiHoa = await _context.LoaiHoas.FindAsync(id);
            if (loaiHoa == null)
            {
                TempData["Loi"] = "Loại hoa không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // KIỂM TRA RÀNG BUỘC: Không xóa vật lý nếu đã có hoa dùng
            var coHoaDung = await _context.HoaTuois.AnyAsync(h => h.MaLoaiHoa == id);
            if (coHoaDung)
            {
                // Tự động chuyển sang ngừng hoạt động (soft-deactivate) để bảo toàn dữ liệu
                loaiHoa.TrangThai = false;
                await _context.SaveChangesAsync();

                TempData["Loi"] = $"Loại hoa '{loaiHoa.TenLoaiHoa}' đang có các giống hoa tươi trực thuộc sử dụng, không thể xóa vĩnh viễn! Hệ thống đã tự động chuyển trạng thái sang 'Ngừng hoạt động'.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu chưa có bất kỳ giống hoa nào liên kết -> Cho phép xóa vật lý an toàn
            _context.LoaiHoas.Remove(loaiHoa);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xóa vĩnh viễn loại hoa '{loaiHoa.TenLoaiHoa}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // GET: /Admin/LoaiHoa/KiemTraTenLoai (Kiểm tra trùng tên Realtime AJAX)
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> KiemTraTenLoai(string tenLoaiHoa, int? maLoaiHoa)
        {
            if (string.IsNullOrWhiteSpace(tenLoaiHoa))
            {
                return Json(new { hopLe = false, thongBao = "Tên loại hoa không được để trống" });
            }

            var query = _context.LoaiHoas.AsQueryable();
            if (maLoaiHoa.HasValue)
            {
                query = query.Where(l => l.MaLoaiHoa != maLoaiHoa.Value);
            }

            var daTonTai = await query.AnyAsync(l => l.TenLoaiHoa.ToLower() == tenLoaiHoa.Trim().ToLower());

            return Json(new
            {
                hopLe = !daTonTai,
                thongBao = daTonTai ? "Tên loại hoa này đã tồn tại!" : "Tên loại hoa hợp lệ."
            });
        }
    }
}
