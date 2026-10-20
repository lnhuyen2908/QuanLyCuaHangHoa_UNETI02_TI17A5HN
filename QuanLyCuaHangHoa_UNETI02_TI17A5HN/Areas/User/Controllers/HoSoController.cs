// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Bổ sung danh sách đơn của chính khách, chi tiết và POST hủy có lịch sử

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

        // GET hồ sơ: chỉ tải dữ liệu của tài khoản đang đăng nhập và các đơn có MaKhachHang tương ứng.
        // GET: /User/Profile or /User/HoSo
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
            if (!maTaiKhoan.HasValue)
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });

            // Include hồ sơ KhachHang để lấy khóa liên kết tới đơn hàng.
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

            // Không tải đơn của khách khác; chỉ đọc đơn cùng lịch sử/thanh toán/giao nhận.
            if (kh != null)
            {
                donHangs = await _db.DonDatHoas
                    .AsNoTracking()
                    .Where(d => d.MaKhachHang == kh.MaKhachHang)
                    .Include(d => d.ChiTietDonHoas)
                        .ThenInclude(c => c.MauSanPham)
                    .Include(d => d.LichSuXuLyDons)
                    .Include(d => d.GiaoNhans)
                    .Include(d => d.ThanhToans)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();
            }

            // Dùng một ViewModel để hiển thị hồ sơ, đơn gần nhất và tab thiết kế riêng.
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


        // POST hủy đơn: chỉ khách sở hữu, đơn còn chờ xác nhận và chưa có thanh toán.
        // Action thuộc HoSoController; bản ZIP gốc có dấu ngoặc đóng lớp đặt sớm.
        [HttpPost("HuyDon"), ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int maDon, string? lyDo)
        {
            var account = HttpContext.Session.GetMaTaiKhoan();
            if (!account.HasValue)
            {
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });
            }

            // Transaction đảm bảo đổi trạng thái và lưu lịch sử cùng thành công.
            await using var tx = await _db.Database.BeginTransactionAsync();

            // Điều kiện MaTaiKhoan ngăn khách đoán mã để hủy đơn của người khác.
            var d = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaDon == maDon && x.KhachHang.MaTaiKhoan == account.Value);

            if (d == null)
            {
                return NotFound();
            }

            if (d.TrangThai != TrangThaiDon.ChoXacNhan
                || await _db.ThanhToans.AnyAsync(x => x.MaDon == maDon))
            {
                TempData["Loi"] = "Chỉ có thể tự hủy đơn chờ xác nhận và chưa thanh toán.";
                return Redirect("/User/Profile#orders");
            }

            var reason = string.IsNullOrWhiteSpace(lyDo) ? "Khách yêu cầu hủy" : lyDo.Trim();
            if (reason.Length > 500)
            {
                TempData["Loi"] = "Lý do hủy tối đa 500 ký tự.";
                return Redirect("/User/Profile#orders");
            }

            // ExecuteUpdate chỉ đổi đơn còn ở trạng thái chờ, tránh ghi lịch sử hủy hai lần.
            var count = await _db.DonDatHoas
                .Where(x => x.MaDon == maDon && x.TrangThai == TrangThaiDon.ChoXacNhan)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(x => x.TrangThai, TrangThaiDon.DaHuy)
                    .SetProperty(x => x.LyDoHuy, reason));

            if (count != 1)
            {
                TempData["Loi"] = "Đơn đã được xử lý, vui lòng tải lại.";
                return Redirect("/User/Profile#orders");
            }

            // Truy vết thao tác hủy bằng người thực hiện, trạng thái cũ/mới và lý do.
            _db.LichSuXuLyDons.Add(new QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities.LichSuXuLyDon
            {
                MaDon = maDon,
                TrangThaiCu = TrangThaiDon.ChoXacNhan,
                TrangThaiMoi = TrangThaiDon.DaHuy,
                ThoiGian = DateTime.Now,
                NguoiThucHien = account.Value,
                GhiChu = reason
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["ThongBao"] = "Đã hủy đơn hàng.";
            return Redirect("/User/Profile#orders");
        }

        // GET chi tiết: lấy kèm chi tiết, ảnh/tên mẫu, lịch sử, thanh toán và giao nhận.
        // Điều kiện WHERE gắn MaTaiKhoan trong Session để không lộ đơn người khác.
        // Trang chi tiết một đơn của khách đăng nhập; không cho đoán mã xem đơn người khác.
        [HttpGet("ChiTietDon")]
        public async Task<IActionResult> ChiTietDon(int maDon)
        {
            var account = HttpContext.Session.GetMaTaiKhoan();
            if (!account.HasValue)
            {
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });
            }

            var d = await _db.DonDatHoas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.ChiTietDonHoas)
                    .ThenInclude(x => x.MauSanPham)
                .Include(x => x.LichSuXuLyDons)
                .Include(x => x.ThanhToans)
                .Include(x => x.GiaoNhans)
                .FirstOrDefaultAsync(x => x.MaDon == maDon && x.KhachHang.MaTaiKhoan == account.Value);

            return d == null
                ? NotFound()
                : View(new DonDatHoaDetailViewModel { Don = d });
        }
    }
}