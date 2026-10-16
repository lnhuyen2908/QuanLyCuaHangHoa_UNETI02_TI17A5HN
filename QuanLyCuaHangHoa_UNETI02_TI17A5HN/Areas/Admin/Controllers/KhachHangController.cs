// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Danh sách, tìm kiếm, lọc, phân trang, chỉnh sửa và khóa/mở khóa khách hàng.

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
    public class KhachHangController : Controller
    {
        private readonly AppDbContext _context;
        public KhachHangController(AppDbContext context) => _context = context;

        // GET: /Admin/KhachHang
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai,
                                               string sapXep = "name_asc", int page = 1)
        {
            const int pageSize = 10;
            var query = _context.KhachHangs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(k => k.HoTen.Contains(tuKhoa) ||
                                         k.SoDienThoai.Contains(tuKhoa) ||
                                         k.Email.Contains(tuKhoa));
            }
            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            // Sắp xếp trước khi Skip/Take để các trang ổn định.
            query = sapXep switch
            {
                "name_desc" => query.OrderByDescending(k => k.HoTen).ThenBy(k => k.MaKhachHang),
                "date_new" => query.OrderByDescending(k => k.MaKhachHang),
                "date_old" => query.OrderBy(k => k.MaKhachHang),
                _ => query.OrderBy(k => k.HoTen).ThenBy(k => k.MaKhachHang)
            };
            if (sapXep != "name_desc" && sapXep != "date_new" && sapXep != "date_old")
                sapXep = "name_asc";

            // PagedList<T> của nhóm tự Count, Skip, Take và kiểm tra trang hợp lệ.
            var danhSach = await PagedList<KhachHang>.CreateAsync(query, page, pageSize);

            return View(new KhachHangIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                TongDangHoatDong = await _context.KhachHangs.CountAsync(k => k.TrangThai),
                TongBiKhoa = await _context.KhachHangs.CountAsync(k => !k.TrangThai)
            });
        }

        // GET: /Admin/KhachHang/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kh = await _context.KhachHangs.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);
            if (kh == null) return NotFound();

            return View(new KhachHangEditViewModel
            {
                MaKhachHang = kh.MaKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi
            });
        }

        // POST: Chỉ cập nhật những trường được phép chỉnh sửa.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHangEditViewModel model)
        {
            if (id != model.MaKhachHang) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);
            if (kh == null) return NotFound();

            // Chuẩn hóa dữ liệu trước khi lưu.
            var hoTen = model.HoTen.Trim();
            var sdt = model.SoDienThoai.Trim();
            var email = model.Email.Trim();
            if (hoTen.Length == 0 || sdt.Length == 0 || email.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Họ tên, số điện thoại và email không được để trống.");
                return View(model);
            }

            // Kiểm tra email trùng với khách hàng khác.
            bool trungEmailKhachHang = await _context.KhachHangs
                .AnyAsync(k =>
                    k.MaKhachHang != id &&
                    k.Email.ToLower() == email.ToLower());

            // Kiểm tra email trùng với tài khoản khác.
            // Loại trừ tài khoản đang liên kết với khách hàng hiện tại.
            bool trungEmailTaiKhoan = await _context.TaiKhoans
                .AnyAsync(t =>
                    t.MaTaiKhoan != kh.MaTaiKhoan &&
                    t.Email.ToLower() == email.ToLower());

            if (trungEmailKhachHang || trungEmailTaiKhoan)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng trong hệ thống."
                );

                return View(model);
            }

            // Cập nhật thông tin khách hàng.
            kh.HoTen = hoTen;
            kh.SoDienThoai = sdt;
            kh.Email = email;
            kh.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();

            // Đồng bộ các trường chung để tránh sai lệch giữa KhachHang và TaiKhoan.
            if (kh.TaiKhoan != null)
            {
                kh.TaiKhoan.HoTen = hoTen;
                kh.TaiKhoan.Email = email;
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Cập nhật thông tin khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Chỉ Admin được khóa/mở khóa tài khoản khách hàng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            // Kiểm tra trực tiếp để bảo vệ Action, không chỉ ẩn nút ở View.
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);
            if (kh == null) return NotFound();

            if (kh.TaiKhoan == null || kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                TempData["ThongBao"] = "Không thể thay đổi trạng thái: tài khoản liên kết không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu tài khoản đã bị khóa độc lập ở Module Tài khoản,
            // không tự ý mở khóa qua Module Khách hàng.
            if (!kh.TrangThai && !kh.TaiKhoan.TrangThai)
            {
                kh.TrangThai = true;
                kh.TaiKhoan.TrangThai = true;
            }
            else if (kh.TrangThai && kh.TaiKhoan.TrangThai)
            {
                kh.TrangThai = false;
                kh.TaiKhoan.TrangThai = false;
            }
            else
            {
                TempData["ThongBao"] = "Trạng thái khách hàng và tài khoản không đồng nhất. Cần kiểm tra trước khi thay đổi.";
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = kh.TrangThai
                ? "Đã mở khóa khách hàng thành công."
                : "Đã khóa khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/KhachHang/Create (Thêm mới khách hàng kèm tạo tài khoản đồng bộ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Create(KhachHangCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var loi = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["ThongBao"] = loi ?? "Dữ liệu nhập vào chưa hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var tenDangNhap = model.TenDangNhap.Trim();
            var email = model.Email.Trim();
            var sdt = model.SoDienThoai.Trim();

            // Kiểm tra trùng username
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap.ToLower() == tenDangNhap.ToLower()))
            {
                TempData["ThongBao"] = $"Tên đăng nhập '{tenDangNhap}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng email
            if (await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == email.ToLower()) ||
                await _context.KhachHangs.AnyAsync(k => k.Email.ToLower() == email.ToLower()))
            {
                TempData["ThongBao"] = $"Email '{email}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng số điện thoại
            if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt))
            {
                TempData["ThongBao"] = $"Số điện thoại '{sdt}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // 1. Tạo Tài khoản Khách hàng
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen.Trim(),
                Email = email,
                VaiTro = VaiTroConst.KhachHang,
                TrangThai = true
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // 2. Tạo Hồ sơ Khách hàng liên kết
            var khachHang = new KhachHang
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                HoTen = taiKhoan.HoTen,
                SoDienThoai = sdt,
                Email = email,
                DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim(),
                TrangThai = true
            };
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Thêm mới khách hàng '{khachHang.HoTen}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/KhachHang/Delete (Xóa khách hàng kèm xóa tài khoản đồng bộ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
            {
                TempData["ThongBao"] = "Không tìm thấy khách hàng cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra nếu khách hàng đã phát sinh đơn đặt hoa
            var coDon = await _context.DonDatHoas.AnyAsync(d => d.MaKhachHang == id);
            if (coDon)
            {
                TempData["ThongBao"] = $"Khách hàng '{kh.HoTen}' đã có đơn đặt hàng trong hệ thống, không thể xóa để bảo toàn dữ liệu! Vui lòng dùng chức năng Khóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tk = kh.TaiKhoan;
                _context.KhachHangs.Remove(kh);

                if (tk != null)
                {
                    _context.TaiKhoans.Remove(tk);
                }

                await _context.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã xóa khách hàng '{kh.HoTen}' và tài khoản liên kết thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ThongBao"] = "Không thể xóa do khách hàng có dữ liệu liên kết khác trong hệ thống. Vui lòng chọn Khóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
