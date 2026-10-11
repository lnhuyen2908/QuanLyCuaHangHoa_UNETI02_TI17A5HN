// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Extension method đọc/ghi Session (Object, String, Int), thông tin đăng nhập và Cookie ghi nhớ tên đăng nhập.

using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers
{
    // Key Session tập trung một chỗ, tránh gõ sai chuỗi ở nhiều nơi
    public static class SessionKeys
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";
    }

    public static class CookieKeys
    {
        public const string TenDangNhapGhiNho = "TenDangNhapGhiNho";
    }

    public static class SessionExtensions
    {
        // ===== Object <-> JSON (dùng cho giỏ hàng, dữ liệu tạm nhỏ) =====
        public static void SetJson<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Trả về default nếu key không tồn tại, Session hết hạn hoặc JSON hỏng
        public static T? GetJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json)) return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        // ===== Thông tin đăng nhập =====
        // Gọi khi đăng nhập thành công. TUYỆT ĐỐI không lưu mật khẩu vào Session.
        // Không gọi Clear() ở đây để không làm mất giỏ hàng khách đã chọn trước khi đăng nhập.
        public static void SetThongTinDangNhap(this ISession session, int maTaiKhoan, string hoTen, string vaiTro)
        {
            session.SetInt32(SessionKeys.MaTaiKhoan, maTaiKhoan);
            session.SetString(SessionKeys.HoTen, hoTen);
            session.SetString(SessionKeys.VaiTro, vaiTro);
        }

        // Chỉ xóa 3 key đăng nhập. Khi Đăng xuất hoàn toàn vẫn dùng Session.Clear().
        public static void XoaThongTinDangNhap(this ISession session)
        {
            session.Remove(SessionKeys.MaTaiKhoan);
            session.Remove(SessionKeys.HoTen);
            session.Remove(SessionKeys.VaiTro);
        }

        public static int? GetMaTaiKhoan(this ISession session)
            => session.GetInt32(SessionKeys.MaTaiKhoan);

        public static string? GetHoTen(this ISession session)
            => session.GetString(SessionKeys.HoTen);

        public static string? GetVaiTro(this ISession session)
            => session.GetString(SessionKeys.VaiTro);

        // Đã đăng nhập = có Mã tài khoản VÀ có Vai trò (tránh trạng thái nửa vời)
        public static bool DaDangNhap(this ISession session)
            => session.GetMaTaiKhoan().HasValue
               && !string.IsNullOrEmpty(session.GetVaiTro());

        // Vai trò hiện tại có nằm trong danh sách cho phép không (logic HOẶC).
        // Chấp nhận cả dạng "Admin","NhanVien" lẫn "Admin,NhanVien".
        public static bool CoVaiTro(this ISession session, params string[] vaiTros)
        {
            var vaiTro = session.GetVaiTro();
            if (string.IsNullOrEmpty(vaiTro)) return false;

            return vaiTros
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Any(v => string.Equals(v, vaiTro, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Cookie "Ghi nhớ đăng nhập": CHỈ lưu tên đăng nhập để điền sẵn vào form.
    // Không lưu mật khẩu, không lưu vai trò (vai trò luôn lấy từ Session/DB).
    public static class CookieExtensions
    {
        public static void GhiNhoTenDangNhap(this HttpContext context, string tenDangNhap, int soNgay = 7)
        {
            var option = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(soNgay),
                HttpOnly = true,                         // chống XSS đọc lén cookie
                Secure = context.Request.IsHttps,        // chỉ gửi qua HTTPS khi site chạy HTTPS
                SameSite = SameSiteMode.Lax,             // giảm rủi ro CSRF
                IsEssential = true
            };
            context.Response.Cookies.Append(CookieKeys.TenDangNhapGhiNho, tenDangNhap, option);
        }

        public static string? DocTenDangNhapDaGhiNho(this HttpContext context)
            => context.Request.Cookies[CookieKeys.TenDangNhapGhiNho];

        public static void XoaTenDangNhapDaGhiNho(this HttpContext context)
            => context.Response.Cookies.Delete(CookieKeys.TenDangNhapGhiNho);
    }
}