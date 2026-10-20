// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Danh sách, tìm kiếm, lọc, phân trang, chỉnh sửa và khóa/mở khóa khách hàng.
//đồng bộ thông tin với tài khoản liên kết và kiểm tra ràng buộc đơn hàng.

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

        public KhachHangController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách khách hàng
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai,
                                                string sapXep = "date_new",
                                                int page = 1)
        {
            const int pageSize = 10;
            var query = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsNoTracking()
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoa) ||
                    k.SoDienThoai.Contains(tuKhoa) ||
                    k.Email.Contains(tuKhoa));
            }

            // Lọc trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(k =>
                    k.TrangThai == trangThai.Value);
            }

            // Sắp xếp
            query = sapXep switch
            {
                "name_desc" => query.OrderByDescending(k => k.HoTen).ThenBy(k => k.MaKhachHang),
                "date_new" => query.OrderByDescending(k => k.MaKhachHang),
                "date_old" => query.OrderBy(k => k.MaKhachHang),
                _ => query.OrderBy(k => k.HoTen).ThenBy(k => k.MaKhachHang)
            };

            if (sapXep != "name_desc" && sapXep != "name_asc" && sapXep != "date_old")
            {
                sapXep = "date_new";
            }

            // Phân trang
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

        // GET: Sửa khách hàng
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kh = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
                return NotFound();

            return View(new KhachHangEditViewModel
            {
                MaKhachHang = kh.MaKhachHang,
                HoTen = kh.HoTen,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi
            });
        }

        // POST: Sửa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  KhachHangEditViewModel model)
        {
            if (id != model.MaKhachHang)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
                return NotFound();

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Khách hàng chưa có tài khoản liên kết hợp lệ!");

                return View(model);
            }

            var hoTen = model.HoTen.Trim();
            var sdt = model.SoDienThoai.Trim();
            var email = model.Email.Trim();

            // Kiểm tra email trùng ở hai bảng
            bool trungEmailKhachHang = await _context.KhachHangs.AnyAsync(k =>
                    k.MaKhachHang != id &&
                    k.Email.ToLower() == email.ToLower());

            bool trungEmailTaiKhoan = await _context.TaiKhoans.AnyAsync(t =>
                    t.MaTaiKhoan != kh.MaTaiKhoan &&
                    t.Email.ToLower() == email.ToLower());

            if (trungEmailKhachHang || trungEmailTaiKhoan)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng trong hệ thống.");
                return View(model);
            }

            // Kiểm tra trùng số điện thoại
            if (await _context.KhachHangs.AnyAsync(k =>
                k.MaKhachHang != id &&
                k.SoDienThoai == sdt))
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại này đã được sử dụng!");
                return View(model);
            }

            // Cập nhật khách hàng
            kh.HoTen = hoTen;
            kh.SoDienThoai = sdt;
            kh.Email = email;
            kh.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();

            // Đồng bộ tài khoản
            kh.TaiKhoan.HoTen = hoTen;
            kh.TaiKhoan.Email = email;

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật khách hàng. Vui lòng thử lại.");
                return View(model);
            }
        }

        // POST: Khóa/mở khóa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
                return NotFound();

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                TempData["ThongBao"] = "Tài khoản liên kết không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trạng thái hai bảng
            if (kh.TrangThai != kh.TaiKhoan.TrangThai)
            {
                TempData["ThongBao"] = "Trạng thái khách hàng và tài khoản không đồng nhất!";
                return RedirectToAction(nameof(Index));
            }

            // Đổi trạng thái cả hai bảng
            bool trangThaiMoi = !kh.TrangThai;

            kh.TrangThai = trangThaiMoi;
            kh.TaiKhoan.TrangThai = trangThaiMoi;

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = trangThaiMoi
                    ? "Mở khóa khách hàng thành công!"
                    : "Khóa khách hàng thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ThongBao"] = "Không thể thay đổi trạng thái khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Thêm khách hàng và tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Create(
            KhachHangCreateViewModel model)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var loi = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage;

                TempData["ThongBao"] = loi ?? "Dữ liệu không hợp lệ!";

                return RedirectToAction(nameof(Index));
            }

            var tenDangNhap = model.TenDangNhap.Trim();
            var hoTen = model.HoTen.Trim();
            var email = model.Email.Trim();
            var sdt = model.SoDienThoai.Trim();

            // Kiểm tra trùng tên đăng nhập
            if (await _context.TaiKhoans.AnyAsync(t =>
                t.TenDangNhap.ToLower() == tenDangNhap.ToLower()))
            {
                TempData["ThongBao"] = $"Tên đăng nhập '{tenDangNhap}' đã tồn tại!";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email trùng ở hai bảng
            if (await _context.TaiKhoans.AnyAsync(t =>
                    t.Email.ToLower() == email.ToLower()) ||
                await _context.KhachHangs.AnyAsync(k =>
                    k.Email.ToLower() == email.ToLower()))
            {
                TempData["ThongBao"] = $"Email '{email}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng số điện thoại
            if (await _context.KhachHangs.AnyAsync(k =>
                k.SoDienThoai == sdt))
            {
                TempData["ThongBao"] = $"Số điện thoại '{sdt}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Tạo tài khoản và khách hàng cùng thành công
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tạo tài khoản
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = model.MatKhau,
                    HoTen = hoTen,
                    Email = email,
                    VaiTro = VaiTroConst.KhachHang,
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Tạo hồ sơ khách hàng
                var khachHang = new KhachHang
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = hoTen,
                    SoDienThoai = sdt,
                    Email = email,
                    DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                        ? null
                        : model.DiaChi.Trim(),
                    TrangThai = true
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] = $"Thêm khách hàng '{hoTen}' thành công!";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Không thể tạo khách hàng và tài khoản!";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Đã xảy ra lỗi khi tạo khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa khách hàng và tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
            {
                TempData["ThongBao"] = "Không tìm thấy khách hàng cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                TempData["ThongBao"] = "Không thể xóa vì tài khoản liên kết không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // Không xóa khách hàng đã có đơn hàng
            bool coDon = await _context.DonDatHoas
                .AnyAsync(d => d.MaKhachHang == id);

            if (coDon)
            {
                TempData["ThongBao"] = $"Khách hàng '{kh.HoTen}' đã có đơn đặt hoa, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Xóa đồng thời hai bảng
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tk = kh.TaiKhoan;

                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();

                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] = $"Đã xóa khách hàng '{kh.HoTen}' và tài khoản liên kết!";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                TempData["ThongBao"] = "Không thể xóa do còn dữ liệu liên kết. Vui lòng sử dụng chức năng Khóa.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Đã xảy ra lỗi khi xóa khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

