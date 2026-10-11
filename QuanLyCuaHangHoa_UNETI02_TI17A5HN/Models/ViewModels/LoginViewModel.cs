// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: ViewModel đăng nhập (kiểm tra dữ liệu đầu vào phía server).

using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class LoginViewModel
    {
        // Seed dùng tên đăng nhập (admin, nhanvien01, khachhang01...).
        // Service có thể cho phép khớp thêm theo Email.
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        [Display(Name = "Tên đăng nhập hoặc Email")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        // Chỉ ghi nhớ TÊN ĐĂNG NHẬP vào Cookie, không bao giờ lưu mật khẩu
        [Display(Name = "Ghi nhớ đăng nhập trên thiết bị này")]
        public bool GhiNho { get; set; }

        // Trang quay lại sau khi đăng nhập (do PhanQuyenAttribute truyền sang).
        // Controller PHẢI kiểm tra Url.IsLocalUrl(ReturnUrl) trước khi Redirect.
        public string? ReturnUrl { get; set; }
    }
}