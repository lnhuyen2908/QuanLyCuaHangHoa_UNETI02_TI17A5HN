using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Profile")]
    [Route("User/Customer")]
    [PhanQuyen]
    public class HoSoController : Controller
    {
        private readonly AppDbContext _db;

        public HoSoController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /User/Profile or /User/HoSo
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            var taiKhoan = await _db.TaiKhoans
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

            if (taiKhoan == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });
            }

            var kh = taiKhoan.KhachHang;
            var donHangs = new List<Models.Entities.DonDatHoa>();

            if (kh != null)
            {
                donHangs = await _db.DonDatHoas
                    .AsNoTracking()
                    .Where(d => d.MaKhachHang == kh.MaKhachHang)
                    .Include(d => d.ChiTietDonHoas)
                        .ThenInclude(c => c.MauSanPham)
                    .Include(d => d.LichSuXuLyDons)
                    .Include(d => d.GiaoNhans)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();
            }

            var vm = new HoSoViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                MaKhachHang = kh?.MaKhachHang,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = kh?.HoTen ?? taiKhoan.HoTen,
                Email = kh?.Email ?? taiKhoan.Email,
                SoDienThoai = kh?.SoDienThoai ?? string.Empty,
                DiaChi = kh?.DiaChi,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai,
                DanhSachDonHang = donHangs,
                DonHangGanNhat = donHangs.FirstOrDefault(),
                DonHangThietKeRieng = donHangs.Where(d => d.ChiTietDonHoas.Any(c => !string.IsNullOrEmpty(c.YeuCauRieng) || c.MaMau == null)).ToList()
            };

            return View("~/Areas/User/Views/HoSo/Index.cshtml", vm);
        }

        // POST: Cập nhật thông tin cá nhân
        [HttpPost("CapNhatThongTin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTin(HoSoViewModel vm)
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            var taiKhoan = await _db.TaiKhoans
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

            if (taiKhoan == null)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            if (string.IsNullOrWhiteSpace(vm.HoTen))
            {
                TempData["Loi"] = "Họ và tên không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                TempData["Loi"] = "Email không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng email với tài khoản khác
            var emailTrung = await _db.TaiKhoans.AnyAsync(t => t.Email == vm.Email.Trim() && t.MaTaiKhoan != maTaiKhoan.Value);
            if (emailTrung)
            {
                TempData["Loi"] = "Email này đã được sử dụng bởi tài khoản khác.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật TaiKhoan
            taiKhoan.HoTen = vm.HoTen.Trim();
            taiKhoan.Email = vm.Email.Trim();

            // Cập nhật KhachHang nếu có
            if (taiKhoan.KhachHang != null)
            {
                taiKhoan.KhachHang.HoTen = vm.HoTen.Trim();
                taiKhoan.KhachHang.Email = vm.Email.Trim();
                taiKhoan.KhachHang.SoDienThoai = vm.SoDienThoai?.Trim() ?? string.Empty;
                taiKhoan.KhachHang.DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim();
            }

            await _db.SaveChangesAsync();

            // Cập nhật lại session
            HttpContext.Session.SetThongTinDangNhap(taiKhoan.MaTaiKhoan, taiKhoan.HoTen, taiKhoan.VaiTro);

            TempData["ThongBao"] = "Cập nhật thông tin hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Đổi mật khẩu
        [HttpPost("DoiMatKhau")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel vm)
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            if (!ModelState.IsValid)
            {
                TempData["Loi"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu nhập không hợp lệ.";
                return Redirect("/User/Profile#security");
            }

            var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);
            if (taiKhoan == null)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            // So sánh mật khẩu hiện tại
            var a = Encoding.UTF8.GetBytes(vm.MatKhauHienTai);
            var b = Encoding.UTF8.GetBytes(taiKhoan.MatKhau);
            if (!CryptographicOperations.FixedTimeEquals(a, b))
            {
                TempData["Loi"] = "Mật khẩu hiện tại không chính xác.";
                return Redirect("/User/Profile#security");
            }

            taiKhoan.MatKhau = vm.MatKhauMoi;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công!";
            return Redirect("/User/Profile#security");
        }
    }
}
