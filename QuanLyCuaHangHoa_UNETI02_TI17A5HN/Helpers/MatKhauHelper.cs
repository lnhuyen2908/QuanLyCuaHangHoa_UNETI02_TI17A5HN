// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Helper mã hóa (băm) mật khẩu an toàn theo chuẩn PBKDF2 với HMAC-SHA256 và salt ngẫu nhiên.

using System.Security.Cryptography;
using System.Text;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers
{
    public static class MatKhauHelper
    {
        private const int SaltSize = 16;         // 128-bit salt
        private const int HashSize = 32;         // 256-bit subkey
        private const int Iterations = 100_000;  // Chuẩn OWASP cho PBKDF2-HMAC-SHA256
        private const string VersionPrefix = "PBKDF2v1";

        /// <summary>
        /// Băm mật khẩu dạng plain text thành chuỗi an toàn kèm salt và metadata.
        /// Định dạng: PBKDF2v1:{iterations}:{saltBase64}:{hashBase64}
        /// </summary>
        public static string MaHoa(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau))
                throw new ArgumentNullException(nameof(matKhau));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(matKhau),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{VersionPrefix}:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Xác thực mật khẩu nhập vào so với mật khẩu đã lưu trong DB.
        /// Hỗ trợ cả mật khẩu đã băm (PBKDF2) và mật khẩu cũ chưa băm (tương thích ngược dữ liệu có sẵn).
        /// </summary>
        public static bool KiemTra(string matKhauNhap, string matKhauLuuTrongDb)
        {
            if (string.IsNullOrEmpty(matKhauNhap) || string.IsNullOrEmpty(matKhauLuuTrongDb))
                return false;

            // Kiểm tra định dạng đã băm PBKDF2
            if (matKhauLuuTrongDb.StartsWith(VersionPrefix + ":", StringComparison.Ordinal))
            {
                var parts = matKhauLuuTrongDb.Split(':');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[1], out int iterations)) return false;

                try
                {
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] storedHash = Convert.FromBase64String(parts[3]);

                    byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(matKhauNhap),
                        salt,
                        iterations,
                        HashAlgorithmName.SHA256,
                        storedHash.Length);

                    return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
                }
                catch
                {
                    return false;
                }
            }

            // Tương thích ngược: Mật khẩu cũ chưa băm (ví dụ dữ liệu seed "123456")
            var a = Encoding.UTF8.GetBytes(matKhauNhap);
            var b = Encoding.UTF8.GetBytes(matKhauLuuTrongDb);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        /// <summary>
        /// Kiểm tra xem mật khẩu hiện tại trong DB có cần nâng cấp lên chuẩn băm mới không
        /// (ví dụ tài khoản cũ chưa băm hoặc số vòng lặp thấp hơn).
        /// </summary>
        public static bool CanNangCap(string matKhauLuuTrongDb)
        {
            if (string.IsNullOrEmpty(matKhauLuuTrongDb)) return true;
            return !matKhauLuuTrongDb.StartsWith(VersionPrefix + ":", StringComparison.Ordinal);
        }
    }
}
