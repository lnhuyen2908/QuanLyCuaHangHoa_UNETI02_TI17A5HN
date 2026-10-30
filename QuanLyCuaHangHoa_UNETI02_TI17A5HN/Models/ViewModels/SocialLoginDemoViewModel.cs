// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: ViewModel cho tính năng đăng nhập & đăng ký nhanh qua Gmail / Zalo và chế độ giả lập môi trường phát triển.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class SocialLoginDemoViewModel
    {
        public string Provider { get; set; } = "Google"; // "Google" hoặc "Zalo"
        public string? ReturnUrl { get; set; }
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public bool IsRegister { get; set; } = false;
        public bool IsConfigured { get; set; }
    }
}
