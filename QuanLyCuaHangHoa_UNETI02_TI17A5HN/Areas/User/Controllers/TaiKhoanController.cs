// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Controller đăng nhập, đăng ký, đăng xuất, trang từ chối truy cập (Session + phân quyền).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [PhanQuyen] // Mặc định phải đăng nhập; các action công khai mở ngoại lệ bằng [AllowAnonymous]
    public class TaiKhoanController : Controller
    {
        private readonly TaiKhoanService _taiKhoanService;

        public TaiKhoanController(TaiKhoanService taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
        }

        // ===================== ĐĂNG NHẬP =====================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (HttpContext.Session.DaDangNhap())
            {
                var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
                if (maTaiKhoan.HasValue && await _taiKhoanService.ConHoatDongAsync(maTaiKhoan.Value))
                {
                    return ChuyenHuongTheoVaiTro(HttpContext.Session.GetVaiTro());
                }
                HttpContext.Session.Clear();
            }

            var vm = new LoginViewModel { ReturnUrl = returnUrl };

            // Điền sẵn tên đăng nhập nếu trước đó đã tích "Ghi nhớ"
            var daGhiNho = HttpContext.DocTenDangNhapDaGhiNho();
            if (!string.IsNullOrEmpty(daGhiNho))
            {
                vm.TenDangNhap = daGhiNho;
                vm.GhiNho = true;
            }
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var kq = await _taiKhoanService.DangNhapAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }

            var tk = kq.DuLieu!;
            HttpContext.Session.SetThongTinDangNhap(tk.MaTaiKhoan, tk.HoTen, tk.VaiTro);

            if (vm.GhiNho) HttpContext.GhiNhoTenDangNhap(tk.TenDangNhap);
            else HttpContext.XoaTenDangNhapDaGhiNho();

            // Chuyển hướng theo vai trò:
            // Admin / NhanVien -> Tự động chuyển hướng ra trang Dashboard Quản trị
            if (tk.VaiTro is VaiTroConst.Admin or VaiTroConst.NhanVien)
            {
                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)
                    && vm.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return LocalRedirect(vm.ReturnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // User / Khách hàng -> Tự động chuyển hướng ra trang Home User
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)
                && !vm.ReturnUrl.Contains("Login", StringComparison.OrdinalIgnoreCase)
                && !vm.ReturnUrl.Contains("Register", StringComparison.OrdinalIgnoreCase)
                && !vm.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(vm.ReturnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        // ===================== ĐĂNG KÝ =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.DaDangNhap())
                return ChuyenHuongTheoVaiTro(HttpContext.Session.GetVaiTro());

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var kq = await _taiKhoanService.DangKyAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }

            TempData["ThanhCong"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        // ===================== ĐĂNG XUẤT =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // đề bài: đăng xuất phải xóa Session
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        // ===================== TỪ CHỐI TRUY CẬP =====================
        // PhanQuyenAttribute chuyển hướng về đây khi sai vai trò
        [AllowAnonymous]
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // ===================== HELPER =====================
        private IActionResult ChuyenHuongTheoVaiTro(string? vaiTro)
        {
            return vaiTro is VaiTroConst.Admin or VaiTroConst.NhanVien
                ? RedirectToAction("Index", "Home", new { area = "Admin" })
                : RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}