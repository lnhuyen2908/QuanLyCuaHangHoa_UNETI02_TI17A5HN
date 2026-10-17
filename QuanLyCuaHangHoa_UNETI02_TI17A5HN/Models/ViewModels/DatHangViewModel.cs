// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng DatHangViewModel phục vụ chức năng đặt hàng
// tiếp nhận và kiểm tra thông tin khách hàng nhập từ giao diện đặt hàng trước khi xử lý tạo đơn.
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DatHangViewModel
    {
        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
        [Display(Name = "Người nhận")]
        public string NguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoaiNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hình thức nhận hàng là bắt buộc")]
        [Display(Name = "Hình thức nhận")]
        public string HinhThucNhan { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ giao hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string? DiaChiGiao { get; set; }

        [Required(ErrorMessage = "Thời gian nhận/giao là bắt buộc")]
        [Display(Name = "Thời gian nhận/giao")]
        public DateTime? ThoiGianNhanGiao { get; set; }

        [Display(Name = "Lời nhắn")]
        public string? LoiNhan { get; set; }
    }
}
