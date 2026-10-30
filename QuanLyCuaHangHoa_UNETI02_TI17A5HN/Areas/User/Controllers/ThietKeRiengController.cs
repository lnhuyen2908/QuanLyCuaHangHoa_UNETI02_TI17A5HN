// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Nhận form thiết kế riêng POST thật; tạo đơn chưa chốt giá, ghi lịch sử.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/CustomDesign")]
    public class ThietKeRiengController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ThietKeRiengController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Lấy danh sách loại hoa đang hoạt động từ database.
        // Dùng chung khi mở form và khi form có lỗi.
        private async Task NapDanhSachLoaiHoa(ThietKeRiengViewModel vm)
        {
            vm.DanhSachLoaiHoa = await _db.LoaiHoas
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.TenLoaiHoa)
                .ToListAsync();
        }

        // GET: hiển thị form thiết kế hoa theo yêu cầu, chưa phát sinh đơn hàng.
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var vm = new ThietKeRiengViewModel();
            var account = HttpContext.Session.GetMaTaiKhoan();

            if (account.HasValue)
            {
                var kh = await _db.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaTaiKhoan == account.Value && x.TrangThai);

                if (kh != null)
                {
                    vm.NguoiNhan = kh.HoTen;
                    vm.SoDienThoai = kh.SoDienThoai;
                }
            }

            // Nạp loại hoa để hiển thị.
            await NapDanhSachLoaiHoa(vm);

            return View(vm);
        }

        // POST: chỉ tạo yêu cầu khi dữ liệu form, thời gian giao và ảnh tham khảo hợp lệ.
        [HttpPost(""), HttpPost("Index"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ThietKeRiengViewModel vm)
        {
            // Nạp lại danh sách loại hoa để giữ giao diện khi form có lỗi.
            await NapDanhSachLoaiHoa(vm);

            // Loại bỏ các mã bị gửi trùng.
            var maDaChon = vm.MaLoaiHoaDaChon.Distinct().ToList();

            // Giới hạn tối đa 4 loại hoa.
            if (maDaChon.Count > 4)
            {
                ModelState.AddModelError(
                    nameof(vm.MaLoaiHoaDaChon),
                    "Chỉ được chọn tối đa 4 loại hoa.");
            }

            // Kiểm tra mã loại hoa thực sự tồn tại và đang hoạt động.
            // Chỉ lấy tên từ database, không tin tên gửi từ trình duyệt.
            var tenHoa = await _db.LoaiHoas
                .AsNoTracking()
                .Where(x => x.TrangThai && maDaChon.Contains(x.MaLoaiHoa))
                .OrderBy(x => x.MaLoaiHoa)
                .Select(x => x.TenLoaiHoa)
                .ToListAsync();

            // Nếu số loại hoa hợp lệ không khớp số mã được gửi lên thì dữ liệu không hợp lệ.
            if (tenHoa.Count != maDaChon.Count)
            {
                ModelState.AddModelError(
                    nameof(vm.MaLoaiHoaDaChon),
                    "Danh sách loại hoa không hợp lệ hoặc đã ngừng hoạt động.");
            }

            // Ghép tên các loại hoa hợp lệ thành chuỗi để lưu yêu cầu.
            ModelState.Remove(nameof(vm.LoaiHoa));
            vm.LoaiHoa = string.Join(", ", tenHoa);

            // Giới hạn độ dài theo ViewModel.
            if (vm.LoaiHoa.Length > 300)
            {
                ModelState.AddModelError(
                    nameof(vm.MaLoaiHoaDaChon),
                    "Tên các loại hoa đã chọn quá dài.");
            }

            // Thời gian giao/nhận tối thiểu cách hiện tại 30 phút để có thời gian xử lý.
            if (vm.ThoiGianNhanGiao == null || vm.ThoiGianNhanGiao <= DateTime.Now.AddMinutes(30))
            {
                ModelState.AddModelError(nameof(vm.ThoiGianNhanGiao), "Chọn thời gian nhận/giao sau ít nhất 30 phút.");
            }

            // Không tin giá trị select từ trình duyệt; bắt buộc nhập địa chỉ khi giao tận nơi.
            if (vm.HinhThucNhan != HinhThucNhanConst.KhachDenNhan
                && vm.HinhThucNhan != HinhThucNhanConst.GiaoTanNoi)
            {
                ModelState.AddModelError(nameof(vm.HinhThucNhan), "Hình thức nhận không hợp lệ.");
            }

            if (vm.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi
                && string.IsNullOrWhiteSpace(vm.DiaChiGiao))
            {
                ModelState.AddModelError(nameof(vm.DiaChiGiao), "Nhập địa chỉ giao hàng.");
            }

            // Bước 1: kiểm tra kích thước, đuôi và MIME của ảnh trước khi đọc nội dung.
            if (vm.AnhThamKhao != null)
            {
                var ext = Path.GetExtension(vm.AnhThamKhao.FileName).ToLowerInvariant();

                if (vm.AnhThamKhao.Length == 0
                    || vm.AnhThamKhao.Length > 5 * 1024 * 1024
                    || !new[] { ".png", ".jpg", ".jpeg", ".webp" }.Contains(ext)
                    || !new[] { "image/png", "image/jpeg", "image/webp" }.Contains(vm.AnhThamKhao.ContentType))
                {
                    ModelState.AddModelError(nameof(vm.AnhThamKhao), "Ảnh chỉ nhận PNG/JPG/WebP tối đa 5MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Chỉ tài khoản có hồ sơ khách hàng đang hoạt động mới được tạo yêu cầu.
            var account = HttpContext.Session.GetMaTaiKhoan();

            if (!account.HasValue)
            {
                return RedirectToAction("Login", "TaiKhoan", new { area = "User" });
            }

            var kh = await _db.KhachHangs
    .AsNoTracking()
    .FirstOrDefaultAsync(x =>
        x.MaTaiKhoan == account.Value && x.TrangThai);

            if (kh == null)
            {
                return Forbid();
            }

            // Bước 2: xác thực chữ ký tệp, sau đó lưu với tên ngẫu nhiên để tránh trùng file.
            string? imgPath = null;
            if (vm.AnhThamKhao != null)
            {
                // Kiểm tra chữ ký file để chặn đổi đuôi phần mở rộng giả mạo.
                await using var stream = vm.AnhThamKhao.OpenReadStream();

                var header = new byte[12];
                var n = await stream.ReadAsync(header);

                var png = n >= 8 && header.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                var jpg = n >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255;
                var webp = n >= 12
                    && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                    && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";

                if (!png && !jpg && !webp)
                {
                    ModelState.AddModelError(nameof(vm.AnhThamKhao), "Ảnh không đúng định dạng.");
                    return View(vm);
                }

                // Đưa luồng về đầu trước khi sao chép ảnh hợp lệ xuống thư mục uploads.
                stream.Position = 0;

                string ext = png ? ".png" : jpg ? ".jpg" : ".webp";
                var path = Path.Combine(_env.WebRootPath, "uploads", "thiet-ke-rieng");

                Directory.CreateDirectory(path);

                var name = Guid.NewGuid().ToString("N") + ext;

                await using var output = System.IO.File.Create(Path.Combine(path, name));
                await stream.CopyToAsync(output);

                imgPath = "/uploads/thiet-ke-rieng/" + name;
            }

            // Tổng hợp thông tin thiết kế vào YeuCauRieng để Admin xem và báo giá.
            string yc = $"Kiểu: {vm.LoaiSanPham}; Tông màu: {vm.TongMau}; Loài hoa: {vm.LoaiHoa}; " +
                        $"Ngân sách: {vm.NganSach}; Thiệp: {vm.NoiDungThiep}; Ghi chú: {vm.GhiChu}; " +
                        $"Ảnh tham khảo: {imgPath}";
            if (yc.Length > 1000)
            {
                yc = yc[..1000];
            }

            var phiGiao = DonHangService.TinhPhiGiaoHang(vm.HinhThucNhan);

            // Chưa có giá thỏa thuận: MaMau = null, DaChotGia = false, DonGia/TongTien = 0.
            // Admin sẽ chốt giá qua YeuCauThietKeController trước khi xác nhận đơn.
            var order = new DonDatHoa
            {
                MaKhachHang = kh.MaKhachHang,
                NgayDat = DateTime.Now,
                HinhThucNhan = vm.HinhThucNhan,
                ThoiGianNhanGiao = vm.ThoiGianNhanGiao!.Value,
                NguoiNhan = vm.NguoiNhan.Trim(),
                SoDienThoaiNguoiNhan = vm.SoDienThoai.Trim(),
                DiaChiGiao = vm.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi ? vm.DiaChiGiao?.Trim() : null,
                LoiNhan = vm.NoiDungThiep,
                TrangThai = TrangThaiDon.ChoXacNhan,
                PhiGiaoHang = phiGiao,
                TongTien = phiGiao, // Chưa bao gồm giá hoa vì Admin chưa báo giá.
                ChiTietDonHoas = new List<ChiTietDonHoa>
            {
                new()
                {
                    MaMau = null,
                    SoLuong = 1,
                    DonGia = 0,
                    ThanhTien = 0,
                    DaChotGia = false,
                    YeuCauRieng = yc
                }
            }
            };

            // Tạo đơn cùng lịch sử trong một transaction, tránh đơn không có dấu vết xử lý.
            await using var tx = await _db.Database.BeginTransactionAsync();

            _db.DonDatHoas.Add(order);
            await _db.SaveChangesAsync();

            _db.LichSuXuLyDons.Add(new LichSuXuLyDon
            {
                MaDon = order.MaDon,
                TrangThaiMoi = TrangThaiDon.ChoXacNhan,
                ThoiGian = DateTime.Now,
                NguoiThucHien = account.Value,
                GhiChu = "Khách gửi yêu cầu thiết kế riêng, chờ báo giá"
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Điều hướng về tab thiết kế riêng trong hồ sơ để khách theo dõi.
            TempData["ThongBao"] = $"Đã gửi yêu cầu thiết kế riêng #{order.MaDon}, đang chờ báo giá.";
            return Redirect("/User/Profile#custom-design");
        }
    }
}
