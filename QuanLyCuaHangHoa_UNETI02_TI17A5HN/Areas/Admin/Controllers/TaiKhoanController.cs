// Họ và tên: Lã Ngọc Huyền
// MSV: 23103100271
// Nội dung thực hiện: Quản trị tài khoản (chỉ Admin): danh sách tìm/lọc, thêm, sửa, khóa/mở khóa, kiểm tra trùng tên đăng nhập.

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
    [PhanQuyen(VaiTroConst.Admin)]
    public class TaiKhoanController : Controller
    {
        private readonly AppDbContext _context;

        public TaiKhoanController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền truy cập: Chỉ có admin mới được quyền truy cập vào trang này
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var vaiTro = HttpContext.Session.GetVaiTro();
            if (vaiTro != VaiTroConst.Admin)
            {
                TempData["Loi"] = "Chỉ có admin mới được quyền truy cập vào trang này";
                context.Result = RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        // GET: /Admin/TaiKhoan/Index (Tìm kiếm, Lọc, Phân trang)
        public async Task<IActionResult> Index(string? tuKhoa, string? vaiTro, bool? trangThai, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.TaiKhoans.Include(t => t.KhachHang).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(tuKhoa)
                                      || t.HoTen.Contains(tuKhoa)
                                      || t.Email.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                query = query.Where(t => t.VaiTro == vaiTro);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(t => t.TrangThai == trangThai.Value);
            }

            query = query.OrderByDescending(t => t.MaTaiKhoan);
            var danhSach = await PagedList<TaiKhoan>.CreateAsync(query, page, pageSize);

            var vm = new TaiKhoanAdminIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                VaiTro = vaiTro,
                TrangThai = trangThai,
                TongTaiKhoan = await _context.TaiKhoans.CountAsync(),
                TongAdmin = await _context.TaiKhoans.CountAsync(t => t.VaiTro == VaiTroConst.Admin),
                TongNhanVien = await _context.TaiKhoans.CountAsync(t => t.VaiTro == VaiTroConst.NhanVien),
                TongKhachHang = await _context.TaiKhoans.CountAsync(t => t.VaiTro == VaiTroConst.KhachHang),
            };

            return View(vm);
        }

   
        // POST: /Admin/TaiKhoan/Create (Thêm mới tài khoản)
        [HttpPost]  //Quy định request được gửi theo cách nào.
        [ValidateAntiForgeryToken] //Kiểm tra token chống giả mạo request CSRF.
        public async Task<IActionResult> Create(TaiKhoanCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var loi = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["Loi"] = loi ?? "Dữ liệu nhập vào chưa hợp lệ, vui lòng kiểm tra lại!";
                return RedirectToAction(nameof(Index));
            }

            var tenDangNhap = vm.TenDangNhap.Trim();
            var email = vm.Email.Trim();

            // Kiểm tra trùng tên đăng nhập
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap.ToLower() == tenDangNhap.ToLower()))
            {
                TempData["Loi"] = $"Tên đăng nhập '{tenDangNhap}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng Email
            if (await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == email.ToLower()))
            {
                TempData["Loi"] = $"Email '{email}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = vm.MatKhau,
                HoTen = vm.HoTen.Trim(),
                Email = email,
                VaiTro = vm.VaiTro,
                TrangThai = true
            };

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // Nếu tạo tài khoản là Khách hàng -> tự động đồng bộ sang bảng KhachHang
            if (vm.VaiTro == VaiTroConst.KhachHang)
            {
                var sdt = string.IsNullOrWhiteSpace(vm.SoDienThoai) ? "0900000000" : vm.SoDienThoai.Trim();
                var khachHang = new KhachHang
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = taiKhoan.HoTen,
                    Email = taiKhoan.Email,
                    SoDienThoai = sdt,
                    DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim(),
                    TrangThai = true
                };
                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();
            }

            TempData["ThanhCong"] = $"Thêm mới tài khoản '{taiKhoan.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/TaiKhoan/Edit (Cập nhật thông tin & vai trò)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaiKhoanEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var loi = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["Loi"] = loi ?? "Dữ liệu chỉnh sửa chưa hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            var tk = await _context.TaiKhoans.FindAsync(vm.MaTaiKhoan);
            if (tk == null)
            {
                TempData["Loi"] = "Không tìm thấy thông tin tài khoản cần sửa!";
                return RedirectToAction(nameof(Index));
            }

            var email = vm.Email.Trim();

            // Kiểm tra trùng Email với tài khoản khác
            if (await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == email.ToLower() && t.MaTaiKhoan != vm.MaTaiKhoan))
            {
                TempData["Loi"] = $"Email '{email}' đã thuộc về tài khoản khác!";
                return RedirectToAction(nameof(Index));
            }

            // Chặn Admin tự hạ quyền của chính mình
            var maDangNhapHienTai = HttpContext.Session.GetMaTaiKhoan();
            if (tk.MaTaiKhoan == maDangNhapHienTai && vm.VaiTro != VaiTroConst.Admin)
            {
                TempData["Loi"] = "Bạn không thể tự hạ quyền Admin của chính tài khoản mình!";
                return RedirectToAction(nameof(Index));
            }

            tk.HoTen = vm.HoTen.Trim();
            tk.Email = email;
            tk.VaiTro = vm.VaiTro;

            if (!string.IsNullOrWhiteSpace(vm.MatKhauMoi))
            {
                if (vm.MatKhauMoi.Length < 6)
                {
                    TempData["Loi"] = "Mật khẩu mới phải có tối thiểu 6 ký tự!";
                    return RedirectToAction(nameof(Index));
                }
                tk.MatKhau = vm.MatKhauMoi;
            }

            // Đồng bộ sang bảng KhachHang
            var kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == tk.MaTaiKhoan);
            if (vm.VaiTro == VaiTroConst.KhachHang)
            {
                if (kh != null)
                {
                    kh.HoTen = tk.HoTen;
                    kh.Email = tk.Email;
                    if (!string.IsNullOrWhiteSpace(vm.SoDienThoai)) kh.SoDienThoai = vm.SoDienThoai.Trim();
                    if (!string.IsNullOrWhiteSpace(vm.DiaChi)) kh.DiaChi = vm.DiaChi.Trim();
                }
                else
                {
                    // Trước đó là vai trò khác nay đổi thành Khách hàng -> tạo hồ sơ Khách hàng
                    kh = new KhachHang
                    {
                        MaTaiKhoan = tk.MaTaiKhoan,
                        HoTen = tk.HoTen,
                        Email = tk.Email,
                        SoDienThoai = string.IsNullOrWhiteSpace(vm.SoDienThoai) ? "0900000000" : vm.SoDienThoai.Trim(),
                        DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim(),
                        TrangThai = tk.TrangThai
                    };
                    _context.KhachHangs.Add(kh);
                }
            }
            else
            {
                // Nếu đổi từ Khách hàng sang vai trò khác (Admin/NhanVien):
                // Nếu khách hàng chưa có đơn hàng thì xóa hồ sơ khách hàng để không hiện bên trang khách hàng nữa
                if (kh != null)
                {
                    var coDon = await _context.DonDatHoas.AnyAsync(d => d.MaKhachHang == kh.MaKhachHang);
                    if (!coDon)
                    {
                        _context.KhachHangs.Remove(kh);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Cập nhật tài khoản '{tk.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }


        // POST: /Admin/TaiKhoan/KhoaMoKhoa (Khóa / Mở khóa tài khoản)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaMoKhoa(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null)
            {
                TempData["Loi"] = "Tài khoản không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Không cho phép Admin tự khóa tài khoản của chính mình
            var maDangNhapHienTai = HttpContext.Session.GetMaTaiKhoan();
            if (tk.MaTaiKhoan == maDangNhapHienTai)
            {
                TempData["Loi"] = "Bạn không thể tự khóa tài khoản đang đăng nhập của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            tk.TrangThai = !tk.TrangThai;

            // Đồng bộ trạng thái sang hồ sơ Khách hàng liên kết
            var kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == tk.MaTaiKhoan);
            if (kh != null)
            {
                kh.TrangThai = tk.TrangThai;
            }

            await _context.SaveChangesAsync();

            var trangThaiText = tk.TrangThai ? "Mở khóa" : "Khóa";
            TempData["ThanhCong"] = $"{trangThaiText} tài khoản '{tk.TenDangNhap}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/TaiKhoan/Delete (Xóa tài khoản)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null)
            {
                TempData["Loi"] = "Tài khoản không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Chặn Admin tự xóa chính mình
            var maDangNhapHienTai = HttpContext.Session.GetMaTaiKhoan();
            if (tk.MaTaiKhoan == maDangNhapHienTai)
            {
                TempData["Loi"] = "Bạn không thể tự xóa tài khoản đang đăng nhập của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Nếu tài khoản liên kết với hồ sơ Khách hàng
                var kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == tk.MaTaiKhoan);
                if (kh != null)
                {
                    // Nếu khách hàng đã từng phát sinh đơn đặt hoa -> chặn xóa để bảo toàn dữ liệu
                    var coDon = await _context.DonDatHoas.AnyAsync(d => d.MaKhachHang == kh.MaKhachHang);
                    if (coDon)
                    {
                        TempData["Loi"] = $"Tài khoản '{tk.TenDangNhap}' đã phát sinh đơn đặt hoa, không thể xóa! Vui lòng dùng chức năng Khóa tài khoản.";
                        return RedirectToAction(nameof(Index));
                    }
                    _context.KhachHangs.Remove(kh);
                }

                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();

                TempData["ThanhCong"] = $"Đã xóa vĩnh viễn tài khoản '{tk.TenDangNhap}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Loi"] = $"Tài khoản '{tk.TenDangNhap}' đã phát sinh dữ liệu trong hệ thống, không thể xóa! Vui lòng dùng chức năng Khóa tài khoản.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/TaiKhoan/KiemTraTenDangNhap (Kiểm tra trùng tên qua AJAX)
        [HttpGet]
        public async Task<IActionResult> KiemTraTenDangNhap(string tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                return Json(new { hopLe = false, thongBao = "Tên đăng nhập không được để trống" });
            }

            var daTonTai = await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap.ToLower() == tenDangNhap.Trim().ToLower());

            return Json(new
            {
                hopLe = !daTonTai,
                thongBao = daTonTai ? "Tên đăng nhập này đã được sử dụng!" : "Tên đăng nhập hợp lệ."
            });
        }

        // GET: /Admin/TaiKhoan/Profile (Hồ sơ Admin cá nhân)
        public async Task<IActionResult> Profile()
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            var tk = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
            if (tk == null)
                return NotFound();

            return View(tk);
        }

        // POST: /Admin/TaiKhoan/Profile (Cập nhật hồ sơ Admin cá nhân)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string hoTen, string email, string? matKhauHienTai, string? matKhauMoi)
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            var tk = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
            if (tk == null) return NotFound();

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email))
            {
                TempData["Loi"] = "Họ tên và Email không được để trống!";
                return View(tk);
            }

            // Kiểm tra trùng email với tài khoản khác
            if (await _context.TaiKhoans.AnyAsync(t => t.Email.ToLower() == email.Trim().ToLower() && t.MaTaiKhoan != tk.MaTaiKhoan))
            {
                TempData["Loi"] = "Email này đã được sử dụng bởi tài khoản khác!";
                return View(tk);
            }

            // Đổi mật khẩu nếu có nhập
            if (!string.IsNullOrWhiteSpace(matKhauMoi))
            {
                if (string.IsNullOrWhiteSpace(matKhauHienTai) || tk.MatKhau != matKhauHienTai)
                {
                    TempData["Loi"] = "Mật khẩu hiện tại không chính xác!";
                    return View(tk);
                }
                if (matKhauMoi.Length < 6)
                {
                    TempData["Loi"] = "Mật khẩu mới phải có tối thiểu 6 ký tự!";
                    return View(tk);
                }
                tk.MatKhau = matKhauMoi;
            }

            tk.HoTen = hoTen.Trim();
            tk.Email = email.Trim();
            await _context.SaveChangesAsync();

            // Cập nhật lại họ tên trong Session
            HttpContext.Session.SetString(SessionKeys.HoTen, tk.HoTen);

            TempData["ThanhCong"] = "Cập nhật thông tin cá nhân thành công!";
            return View(tk);
        }
    }
}
