// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Checkout từ giỏ Session, xác thực dữ liệu và tạo DonDatHoa/ChiTietDonHoa/LichSuXuLyDon
using Microsoft.AspNetCore.Mvc;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Checkout")]
    [PhanQuyen]
    public class DatHangController : Controller
    {
        private readonly AppDbContext _db;
        private readonly DonHangService _don;
        private readonly GioHangService _cart;
        public DatHangController(AppDbContext db, DonHangService don, GioHangService cart)
        {
            _db = db;
            _don = don;
            _cart = cart;
        }

        // Dùng lại service giỏ để cả GET và POST xem cùng nguồn giá hiện tại.
        private async Task<List<CartDisplayItem>> LoadAsync()
            => await _cart.DisplayAsync(HttpContext.Session, HttpContext.Session.GetMaTaiKhoan()!.Value);

        // GET Checkout: yêu cầu giỏ có sản phẩm và điền sẵn thông tin khách từ DB.
        [HttpGet(""), HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var items = await LoadAsync();
            if (items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return Redirect("/User/Cart");
            }

            var kh = await _db.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == HttpContext.Session.GetMaTaiKhoan() && x.TrangThai);

            if (kh == null) return Forbid();

            // ViewBag chứa thông tin chỉ để HIỂN THỊ, không quyết định giá hay mã khách khi tạo đơn.
            ViewBag.Items = items;
            ViewBag.TenNguoiDat = kh.HoTen;
            ViewBag.SdtNguoiDat = kh.SoDienThoai;
            ViewBag.EmailNguoiDat = kh.Email;

            return View(new DatHangViewModel
            {
                NguoiNhan = kh.HoTen,
                SoDienThoaiNguoiNhan = kh.SoDienThoai,
                DiaChiGiao = kh.DiaChi,
                HinhThucNhan = HinhThucNhanConst.GiaoTanNoi
            });
        }

        // POST Checkout: xác thực lại toàn bộ dữ liệu vì form và giỏ có thể thay đổi sau GET.
        [HttpPost(""), HttpPost("Index"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DatHangViewModel vm)
        {
            var account = HttpContext.Session.GetMaTaiKhoan()!.Value;
            var kh = await _db.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == account && x.TrangThai);

            if (kh == null) return Forbid();

            // Chuẩn hóa dữ liệu chuỗi để tránh lưu khoảng trắng thừa.
            vm.NguoiNhan = vm.NguoiNhan?.Trim() ?? "";
            vm.SoDienThoaiNguoiNhan = vm.SoDienThoaiNguoiNhan?.Trim() ?? "";
            vm.DiaChiGiao = vm.DiaChiGiao?.Trim();
            vm.LoiNhan = vm.LoiNhan?.Trim();

            // Kiểm tra nghiệp vụ: chỉ hai hình thức nhận, giao tận nơi cần địa chỉ.
            if (vm.HinhThucNhan != HinhThucNhanConst.GiaoTanNoi
                && vm.HinhThucNhan != HinhThucNhanConst.KhachDenNhan)
            {
                ModelState.AddModelError(nameof(vm.HinhThucNhan), "Hình thức nhận không hợp lệ.");
            }

            if (vm.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi
                && string.IsNullOrWhiteSpace(vm.DiaChiGiao))
            {
                ModelState.AddModelError(nameof(vm.DiaChiGiao), "Nhập địa chỉ để giao tận nơi.");
            }

            if (!vm.ThoiGianNhanGiao.HasValue
                || vm.ThoiGianNhanGiao.Value <= DateTime.Now.AddMinutes(30))
            {
                ModelState.AddModelError(nameof(vm.ThoiGianNhanGiao), "Chọn thời gian nhận/giao sau ít nhất 30 phút.");
            }

            // Đọc lại giỏ và trạng thái mẫu từ DB tại thời điểm POST; loại bỏ giỏ không hợp lệ.
            var items = await LoadAsync();
            if (items.Count == 0 || items.Any(x => !x.ConBan || x.SoLuong is < 1 or > 100))
            {
                ModelState.AddModelError("", "Giỏ trống hoặc chứa mẫu không còn bán. Vui lòng kiểm tra giỏ hàng.");
            }

            // Có lỗi thì giữ dữ liệu người dùng đã nhập và nạp lại danh sách giỏ để vẽ View.
            if (!ModelState.IsValid)
            {
                ViewBag.Items = items;
                ViewBag.TenNguoiDat = kh.HoTen;
                ViewBag.SdtNguoiDat = kh.SoDienThoai;
                ViewBag.EmailNguoiDat = kh.Email;
                return View(vm);
            }

            // Luôn lấy giá hiện tại từ database, không nhận giá và tổng tiền từ trình duyệt/Session.
            var cts = items.Select(x => new ChiTietDonHoa
            {
                MaMau = x.MaMau,
                SoLuong = x.SoLuong,
                DonGia = x.DonGia,
                ThanhTien = _don.TinhThanhTien(x.SoLuong, x.DonGia),
                DaChotGia = true
            }).ToList();

            // Module 4 chưa cung cấp biểu phí; tạm để 0, không tự đặt mức phí.
            var phiGiao = vm.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi ? 0m : 0m; // Phí chưa cấu hình, không giả định biểu phí.

            // Một đơn gồm thông tin người nhận và các chi tiết với giá đã tính tại server.
            var order = new DonDatHoa
            {
                MaKhachHang = kh.MaKhachHang,
                NgayDat = DateTime.Now,
                NguoiNhan = vm.NguoiNhan,
                SoDienThoaiNguoiNhan = vm.SoDienThoaiNguoiNhan,
                HinhThucNhan = vm.HinhThucNhan,
                ThoiGianNhanGiao = vm.ThoiGianNhanGiao!.Value,
                DiaChiGiao = vm.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi ? vm.DiaChiGiao : null,
                LoiNhan = vm.LoiNhan,
                TrangThai = TrangThaiDon.ChoXacNhan,
                PhiGiaoHang = phiGiao,
                TongTien = _don.TinhTongTien(cts.Select(x => x.ThanhTien), phiGiao, 0),
                ChiTietDonHoas = cts
            };

            // Transaction: tạo đơn, các chi tiết và lịch sử phải thành công cùng nhau.
            await using var tx = await _db.Database.BeginTransactionAsync();

            _db.DonDatHoas.Add(order);
            await _db.SaveChangesAsync();

            _db.LichSuXuLyDons.Add(new LichSuXuLyDon
            {
                MaDon = order.MaDon,
                TrangThaiCu = null,
                TrangThaiMoi = TrangThaiDon.ChoXacNhan,
                ThoiGian = DateTime.Now,
                NguoiThucHien = account,
                GhiChu = "Khách hàng đặt hoa qua giỏ Session"
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Chỉ xóa giỏ sau Commit; nếu lưu DB lỗi, giỏ vẫn còn để khách thử lại.
            _cart.Clear(HttpContext.Session, account);

            TempData["ThongBao"] = $"Đặt hàng thành công: #{order.MaDon}. Đơn đang chờ xác nhận.";
            return Redirect("/User/Profile#orders");
        }
    }
}
