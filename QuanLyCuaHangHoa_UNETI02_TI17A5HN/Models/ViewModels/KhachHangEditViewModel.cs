// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Dữ liệu và kiểm tra hợp lệ khi chỉnh sửa khách hàng.

using System.ComponentModel.DataAnnotations;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class KhachHangEditViewModel
    {
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Họ tên khách hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}
