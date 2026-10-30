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
        public async Task<IActionResult> Index([FromQuery] List<int>? maMauChon,
                                                    bool chonTuGio = false,
                                                    bool muaNgay = false)
        {
            var items = muaNgay
                    ? await LoadMuaNgayAsync()
                    : await LoadAsync();

            // Mua ngay nhưng không tìm thấy mẫu hợp lệ.
            if (muaNgay && (items.Count == 0 || items.Any(x => !x.ConBan)))
            {
                TempData["Error"] = "Mẫu mua ngay không còn hợp lệ.";
                return Redirect("/User/MauSanPham");
            }

            ViewBag.MuaNgay = muaNgay;

            // Nếu khách chọn một phần giỏ, chỉ lấy các mẫu đã chọn.
            if (!muaNgay && chonTuGio)
            {
                var ids = maMauChon?.Distinct().ToList() ?? new();

                items = items
                    .Where(x => ids.Contains(x.MaMau))
                    .ToList();

                if (ids.Count == 0 || ids.Count != items.Count)
                {
                    TempData["Error"] = "Vui lòng chọn mẫu hoa hợp lệ.";
                    return Redirect("/User/Cart");
                }
            }

            ViewBag.ChonTuGio = chonTuGio;
            ViewBag.MaMauChon = items.Select(x => x.MaMau).ToList();
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
            ViewBag.DiaChiNguoiDat = kh.DiaChi ?? "";

            return View(new DatHangViewModel
            {
                NguoiNhanLaNguoiDat = true,
                NguoiNhan = kh.HoTen,
                SoDienThoaiNguoiNhan = kh.SoDienThoai,
                DiaChiGiao = kh.DiaChi,
                HinhThucNhan = HinhThucNhanConst.GiaoTanNoi
            });
        }

        // POST Checkout: xác thực lại toàn bộ dữ liệu vì form và giỏ có thể thay đổi sau GET.
        [HttpPost(""), HttpPost("Index"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
                        DatHangViewModel vm,
                        [FromForm] List<int>? maMauChon,
                        [FromForm] bool chonTuGio = false,
                        [FromForm] bool muaNgay = false)
        {
            var account = HttpContext.Session.GetMaTaiKhoan()!.Value;
            var kh = await _db.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == account && x.TrangThai);

            if (kh == null) return Forbid();

            // Khi khách chọn nhận thay cho chính mình, dùng dữ liệu hồ sơ đã đọc từ DB.
            // Không dựa vào tên/số điện thoại do trình duyệt gửi lên trong trường hợp này.
            if (vm.NguoiNhanLaNguoiDat)
            {
                vm.NguoiNhan = kh.HoTen;
                vm.SoDienThoaiNguoiNhan = kh.SoDienThoai;
            }

            // Chuẩn hóa dữ liệu chuỗi để tránh lưu khoảng trắng thừa.
            vm.NguoiNhan = vm.NguoiNhan?.Trim() ?? "";
            vm.SoDienThoaiNguoiNhan = vm.SoDienThoaiNguoiNhan?.Trim() ?? "";
            vm.DiaChiGiao = vm.DiaChiGiao?.Trim();
            vm.LoiNhan = vm.LoiNhan?.Trim();

            // Nguyễn Thị Đào - 23103100245
            // Giai đoạn hiện tại chỉ cho thanh toán COD.
            // Kiểm tra phía server để chặn POST giả mạo.
            if (vm.PhuongThucThanhToan !=
                PhuongThucThanhToanConst.TienMat)
            {
                ModelState.AddModelError(
                    nameof(vm.PhuongThucThanhToan),
                    "Hiện chỉ hỗ trợ thanh toán tiền mặt khi nhận hoa.");
            }


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
            var items = muaNgay
                ? await LoadMuaNgayAsync()
                : await LoadAsync();

            ViewBag.MuaNgay = muaNgay;
            if (!muaNgay && chonTuGio)
            {
                var ids = maMauChon?.Distinct().ToList() ?? new();

                items = items
                    .Where(x => ids.Contains(x.MaMau))
                    .ToList();

                if (ids.Count == 0 || ids.Count != items.Count)
                {
                    ModelState.AddModelError(
                        "",
                        "Danh sách mẫu được chọn không hợp lệ.");
                }
            }

            // Giữ danh sách mẫu đã chọn nếu form đặt hàng có lỗi.
            ViewBag.ChonTuGio = chonTuGio;
            ViewBag.MaMauChon = items.Select(x => x.MaMau).ToList();
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
                ViewBag.DiaChiNguoiDat = kh.DiaChi ?? "";
                return View(vm);
            }

            // Luôn lấy giá hiện tại từ database, không nhận giá và tổng tiền từ trình duyệt/Session.
            var cts = items.Select(x => new ChiTietDonHoa
            {
                MaMau = x.MaMau,
                SoLuong = x.SoLuong,
                DonGia = x.DonGia,
                // Ghép lựa chọn riêng của dòng vào trường YeuCauRieng đã có sẵn trong database.
                // Cách này giữ được nội dung theo từng chi tiết mà không thêm cột/bảng mới.
                YeuCauRieng = TaoYeuCauRieng(x.GiayGoiRuyBang, x.LoiChuc),
                ThanhTien = _don.TinhThanhTien(x.SoLuong, x.DonGia),
                DaChotGia = true
            }).ToList();

            var phiGiao = DonHangService.TinhPhiGiaoHang(vm.HinhThucNhan);

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
                // Nguyễn Thị Đào - 23103100245
                // Ghi nhận lựa chọn COD, chưa thu tiền.
                GhiChu = "Phương thức thanh toán dự kiến: Tiền mặt khi nhận hoa (COD). Chưa thu tiền.",
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
                GhiChu = muaNgay
                            ? "Khách hàng đặt hoa ngay"
                            : "Khách hàng đặt hoa qua giỏ Session"
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            if (muaNgay)
            {
                // Mua ngay: chỉ xóa lựa chọn tạm.
                HttpContext.Session.Remove(MuaNgayKey);
            }
            else if (chonTuGio)
            {
                // Đặt từ giỏ: chỉ xóa các mẫu vừa đặt.
                var maMauDaDat = items.Select(x => x.MaMau).ToHashSet();

                var gioConLai = _cart.Read(HttpContext.Session, account)
                    .Where(x => !maMauDaDat.Contains(x.MaMau))
                    .ToList();

                _cart.Save(HttpContext.Session, account, gioConLai);
            }
            else
            {
                // Đặt toàn bộ giỏ.
                _cart.Clear(HttpContext.Session, account);
            }

            // Nguyễn Thị Đào - 23103100245
            TempData["ThongBao"] =
                $"Đặt hàng thành công: #{order.MaDon}. " +
                "Thanh toán tiền mặt khi nhận hoa. Đơn đang chờ xác nhận.";
            return Redirect("/User/Profile#orders");
        }

        // Lưu giấy gói và lời chúc theo từng dòng dưới dạng văn bản dễ đọc trong cột hiện có.
        private static string? TaoYeuCauRieng(string? giayGoiRuyBang, string? loiChuc)
        {
            var noiDung = new List<string>();
            if (!string.IsNullOrWhiteSpace(giayGoiRuyBang))
                noiDung.Add($"Giấy gói & ruy-băng: {giayGoiRuyBang.Trim()}");
            if (!string.IsNullOrWhiteSpace(loiChuc))
                noiDung.Add($"Lời chúc riêng: {loiChuc.Trim()}");

            return noiDung.Count == 0 ? null : string.Join("\n", noiDung);
        }

        // Session riêng cho mua ngay, không ảnh hưởng giỏ hàng.
        private string MuaNgayKey =>
            $"MuaNgay_{HttpContext.Session.GetMaTaiKhoan()}";

        // Nhận mẫu hoa và các tùy chọn từ trang chi tiết.
        [HttpPost("BuyNow")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(
            int maMau,
            int soLuong,
            string? giayGoiRuyBang,
            string? loiChuc)
        {
            // Kiểm tra dữ liệu khách gửi.
            if (soLuong is < 1 or > 20 ||
                (giayGoiRuyBang?.Length ?? 0) > 100 ||
                (loiChuc?.Length ?? 0) > 180)
            {
                TempData["Error"] = "Thông tin đặt hoa không hợp lệ.";
                return Redirect("/User/MauSanPham");
            }

            // Kiểm tra mẫu hoa thực sự còn bán.
            bool conBan = await _db.MauSanPhams
                .AnyAsync(x => x.MaMau == maMau && x.TrangThai);

            if (!conBan)
            {
                TempData["Error"] = "Mẫu hoa không còn bán.";
                return Redirect("/User/MauSanPham");
            }

            // Lưu tạm lựa chọn mua ngay, không thêm vào giỏ.
            var item = new CartItem
            {
                MaMau = maMau,
                SoLuong = soLuong,
                GiayGoiRuyBang = giayGoiRuyBang?.Trim() ?? "",
                LoiChuc = loiChuc?.Trim() ?? ""
            };

            HttpContext.Session.SetJson(MuaNgayKey, item);

            // Chuyển đến Checkout để nhập thông tin giao nhận.
            return Redirect("/User/Checkout?muaNgay=true");
        }

        // Lấy mẫu mua ngay từ Session và đọc giá hiện tại từ DB.
        private async Task<List<CartDisplayItem>> LoadMuaNgayAsync()
        {
            var item = HttpContext.Session.GetJson<CartItem>(MuaNgayKey);

            if (item == null)
                return new();

            var mau = await _db.MauSanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaMau == item.MaMau);

            if (mau == null)
                return new();

            return new List<CartDisplayItem>
    {
        new CartDisplayItem
        {
            MaMau = mau.MaMau,
            TenMau = mau.TenMau,
            HinhAnh = mau.HinhAnh,
            DonGia = mau.GiaCoBan,
            SoLuong = item.SoLuong,
            GiayGoiRuyBang = item.GiayGoiRuyBang,
            LoiChuc = item.LoiChuc,
            ConBan = mau.TrangThai
        }
    };
        }
    }
}
