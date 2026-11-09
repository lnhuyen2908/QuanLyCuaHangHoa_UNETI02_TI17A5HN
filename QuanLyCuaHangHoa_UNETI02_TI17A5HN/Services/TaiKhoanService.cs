// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Nghiệp vụ đăng nhập, đăng ký tài khoản khách hàng, kiểm tra trạng thái tài khoản.

using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;            // VaiTroConst
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    // Kết quả trả về cho Controller: thành công kèm dữ liệu, hoặc thất bại kèm lỗi.
    // Truong dùng để Controller gắn lỗi đúng ô nhập (ModelState.AddModelError(Truong ?? "", Loi)).
    public class KetQuaDichVu<T>
    {
        public bool ThanhCong { get; private init; }
        public string? Loi { get; private init; }
        public string? Truong { get; private init; }
        public T? DuLieu { get; private init; }

        public static KetQuaDichVu<T> Ok(T duLieu)
            => new() { ThanhCong = true, DuLieu = duLieu };

        public static KetQuaDichVu<T> That(string loi, string? truong = null)
            => new() { ThanhCong = false, Loi = loi, Truong = truong };
    }

    public class TaiKhoanService
    {
        private readonly AppDbContext _db;

        public TaiKhoanService(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // ĐĂNG NHẬP
        // ============================================================
        public async Task<KetQuaDichVu<TaiKhoan>> DangNhapAsync(LoginViewModel vm)
        {
            var dinhDanh = vm.TenDangNhap.Trim();

            // Cho phép đăng nhập bằng Tên đăng nhập hoặc Email
            var tk = await _db.TaiKhoans
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.TenDangNhap == dinhDanh || t.Email == dinhDanh);

            // Không tồn tại hoặc sai mật khẩu -> CÙNG một thông báo,
            // để kẻ tấn công không dò được tên đăng nhập nào có thật.
            if (tk == null || !MatKhauHelper.KiemTra(vm.MatKhau, tk.MatKhau))
                return KetQuaDichVu<TaiKhoan>.That("Tên đăng nhập hoặc mật khẩu không đúng");

            // Nâng cấp mật khẩu tự động sang chuẩn mã hóa PBKDF2 nếu đang ở dạng cũ
            if (MatKhauHelper.CanNangCap(tk.MatKhau))
            {
                tk.MatKhau = MatKhauHelper.MaHoa(vm.MatKhau);
                await _db.SaveChangesAsync();
            }

            // Chỉ báo "bị khóa" SAU KHI mật khẩu đúng (không lộ trạng thái cho người lạ)
            if (!tk.TrangThai)
                return KetQuaDichVu<TaiKhoan>.That("Tài khoản đã bị khóa, vui lòng liên hệ cửa hàng");

            // Khách hàng bị ngừng hoạt động ở hồ sơ KhachHang cũng không được vào
            if (tk.VaiTro == VaiTroConst.KhachHang && tk.KhachHang is { TrangThai: false })
                return KetQuaDichVu<TaiKhoan>.That("Hồ sơ khách hàng đã ngừng hoạt động, vui lòng liên hệ cửa hàng");

            return KetQuaDichVu<TaiKhoan>.Ok(tk);
        }

        // ============================================================
        // ĐĂNG KÝ KHÁCH HÀNG (tạo TaiKhoan + KhachHang trong 1 lần lưu)
        // ============================================================
        public async Task<KetQuaDichVu<TaiKhoan>> DangKyAsync(RegisterViewModel vm)
        {
            var tenDangNhap = vm.TenDangNhap.Trim();
            var email = vm.Email.Trim();
            var soDienThoai = vm.SoDienThoai.Trim();

            if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
                return KetQuaDichVu<TaiKhoan>.That("Tên đăng nhập đã tồn tại", nameof(vm.TenDangNhap));

            if (await _db.TaiKhoans.AnyAsync(t => t.Email == email))
                return KetQuaDichVu<TaiKhoan>.That("Email đã được sử dụng", nameof(vm.Email));

            if (await _db.KhachHangs.AnyAsync(k => k.SoDienThoai == soDienThoai))
                return KetQuaDichVu<TaiKhoan>.That("Số điện thoại đã được sử dụng", nameof(vm.SoDienThoai));

            // Mật khẩu được băm an toàn theo chuẩn PBKDF2 SHA-256
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = MatKhauHelper.MaHoa(vm.MatKhau),
                HoTen = vm.HoTen.Trim(),
                Email = email,
                VaiTro = VaiTroConst.KhachHang,
                TrangThai = true,
                KhachHang = new KhachHang
                {
                    HoTen = vm.HoTen.Trim(),
                    SoDienThoai = soDienThoai,
                    Email = email,
                    DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim(),
                    TrangThai = true
                }
            };

            _db.TaiKhoans.Add(taiKhoan);

            try
            {
                // Một SaveChanges = một transaction: hoặc có cả 2 bản ghi, hoặc không có gì
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Hai người đăng ký cùng tên cùng lúc: kiểm tra AnyAsync ở trên đều qua,
                // nhưng Unique Index ở DB sẽ chặn người đến sau
                return KetQuaDichVu<TaiKhoan>.That("Tên đăng nhập đã tồn tại", nameof(vm.TenDangNhap));
            }

            return KetQuaDichVu<TaiKhoan>.Ok(taiKhoan);
        }

        // ============================================================
        // TIỆN ÍCH CHO CÁC SERVICE KHÁC
        // ============================================================

        // Đổi MaTaiKhoan (lấy từ Session) sang MaKhachHang.
        // Dùng để so với DonDatHoa.MaKhachHang, chặn khách xem/sửa đơn của người khác.
        public async Task<int?> LayMaKhachHangAsync(int maTaiKhoan)
        {
            return await _db.KhachHangs
                .Where(k => k.MaTaiKhoan == maTaiKhoan)
                .Select(k => (int?)k.MaKhachHang)
                .FirstOrDefaultAsync();
        }

        // Session chỉ là bộ đệm: tài khoản có thể bị khóa SAU khi đã đăng nhập.
        // Gọi hàm này trước các thao tác nhạy cảm (đặt hàng, hủy đơn, đổi giá).
        public async Task<bool> ConHoatDongAsync(int maTaiKhoan)
        {
            return await _db.TaiKhoans
                .AnyAsync(t => t.MaTaiKhoan == maTaiKhoan && t.TrangThai);
        }

        // ============================================================
        // ĐĂNG NHẬP / LIÊN KẾT TÀI KHOẢN MẠNG XÃ HỘI (GMAIL & ZALO)
        // ============================================================
        public async Task<KetQuaDichVu<TaiKhoan>> XuLyDangNhapSocialAsync(
            string provider,
            string email,
            string hoTen,
            string? providerUserId = null,
            string? soDienThoai = null,
            string? diaChi = null)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                // Nếu nhà cung cấp không trả email (ví dụ Zalo chưa cấp quyền email), tạo email định danh
                string safeUid = !string.IsNullOrWhiteSpace(providerUserId)
                    ? new string(providerUserId.Where(char.IsLetterOrDigit).ToArray())
                    : RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                email = $"{provider.ToLowerInvariant()}_{safeUid}@social.atelier.vn";
            }

            hoTen = string.IsNullOrWhiteSpace(hoTen) ? $"Người dùng {provider}" : hoTen.Trim();

            // 1. Kiểm tra tài khoản đã tồn tại theo Email chưa
            var tk = await _db.TaiKhoans
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.Email == email);

            if (tk != null)
            {
                // Kiểm tra trạng thái tài khoản
                if (!tk.TrangThai)
                    return KetQuaDichVu<TaiKhoan>.That("Tài khoản liên kết với email này đã bị khóa, vui lòng liên hệ cửa hàng");

                if (tk.VaiTro == VaiTroConst.KhachHang && tk.KhachHang is { TrangThai: false })
                    return KetQuaDichVu<TaiKhoan>.That("Hồ sơ khách hàng đã ngừng hoạt động, vui lòng liên hệ cửa hàng");

                // Nếu là khách hàng nhưng chưa có hồ sơ KhachHang, tự tạo bổ sung
                if (tk.VaiTro == VaiTroConst.KhachHang && tk.KhachHang == null)
                {
                    string fallbackPhone = "09" + RandomNumberGenerator.GetInt32(10000000, 99999999);
                    while (await _db.KhachHangs.AnyAsync(k => k.SoDienThoai == fallbackPhone))
                    {
                        fallbackPhone = "09" + RandomNumberGenerator.GetInt32(10000000, 99999999);
                    }

                    tk.KhachHang = new KhachHang
                    {
                        HoTen = tk.HoTen,
                        Email = tk.Email,
                        SoDienThoai = !string.IsNullOrWhiteSpace(soDienThoai) ? soDienThoai.Trim() : fallbackPhone,
                        DiaChi = diaChi ?? "Chưa cập nhật địa chỉ",
                        TrangThai = true
                    };
                    await _db.SaveChangesAsync();
                }

                return KetQuaDichVu<TaiKhoan>.Ok(tk);
            }

            // 2. Nếu chưa có -> Tạo tài khoản Khách hàng mới tự động
            string baseUsername = email.Split('@')[0];
            baseUsername = new string(baseUsername.Where(char.IsLetterOrDigit).ToArray());
            if (baseUsername.Length < 4) baseUsername = provider.ToLowerInvariant() + "_" + baseUsername;
            if (baseUsername.Length > 30) baseUsername = baseUsername[..30];

            string tenDangNhap = baseUsername;
            int counter = 1;
            while (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
            {
                tenDangNhap = $"{baseUsername}_{counter++}";
                if (tenDangNhap.Length > 50)
                {
                    tenDangNhap = $"user_{RandomNumberGenerator.GetInt32(10000, 99999)}";
                }
            }

            // Mật khẩu ngẫu nhiên bảo mật cao và mã hóa theo chuẩn PBKDF2
            string matKhauNgauNhien = Guid.NewGuid().ToString("N");
            string matKhauDaMaHoa = MatKhauHelper.MaHoa(matKhauNgauNhien);

            // Số điện thoại khách hàng
            string sdt = !string.IsNullOrWhiteSpace(soDienThoai)
                ? soDienThoai.Trim()
                : ("09" + RandomNumberGenerator.GetInt32(10000000, 99999999));

            int phoneCounter = 1;
            while (await _db.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt))
            {
                sdt = "09" + RandomNumberGenerator.GetInt32(10000000, 99999999);
                phoneCounter++;
                if (phoneCounter > 30) break;
            }

            var taiKhoanMoi = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = matKhauDaMaHoa,
                HoTen = hoTen,
                Email = email,
                VaiTro = VaiTroConst.KhachHang,
                TrangThai = true,
                KhachHang = new KhachHang
                {
                    HoTen = hoTen,
                    SoDienThoai = sdt,
                    Email = email,
                    DiaChi = diaChi ?? "Chưa cập nhật địa chỉ",
                    TrangThai = true
                }
            };

            _db.TaiKhoans.Add(taiKhoanMoi);
            await _db.SaveChangesAsync();

            return KetQuaDichVu<TaiKhoan>.Ok(taiKhoanMoi);
        }
    }
}