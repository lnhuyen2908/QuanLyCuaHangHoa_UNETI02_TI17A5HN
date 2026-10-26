// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng DatHangViewModel phục vụ chức năng đặt hàng
// tiếp nhận và kiểm tra thông tin khách hàng nhập từ giao diện đặt hàng trước khi xử lý tạo đơn.
using System.ComponentModel.DataAnnotations;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DatHangViewModel
    {
        // Họ và tên: Nguyễn Thị Đào
        // Mã sinh viên: 23103100245
        // Nội dung: Bổ sung lựa chọn thanh toán COD
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; }
            = PhuongThucThanhToanConst.TienMat;
        // KẾT THÚC BỔ SUNG - NGUYỄN THỊ ĐÀO

        // Chỉ nhận thông tin giao/nhận từ khách; giá và mã khách được lấy ở server.
        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
        [Display(Name = "Người nhận")]
        public string NguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoaiNguoiNhan { get; set; } = string.Empty;

        // Controller kiểm tra lại chỉ nhận hai giá trị hợp lệ (giao tận nơi/tự đến nhận).
        [Required(ErrorMessage = "Hình thức nhận hàng là bắt buộc")]
        [Display(Name = "Hình thức nhận")]
        public string HinhThucNhan { get; set; } = string.Empty;

        // Địa chỉ có thể để trống nếu khách chọn tự đến nhận tại cửa hàng.
        [Display(Name = "Địa chỉ giao hàng")]
        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        public string? DiaChiGiao { get; set; }

        // Thời gian được kiểm tra thêm ở Controller để không nhận lịch quá sát hiện tại.
        [Required(ErrorMessage = "Thời gian nhận/giao là bắt buộc")]
        [Display(Name = "Thời gian nhận/giao")]
        public DateTime? ThoiGianNhanGiao { get; set; }

        [Display(Name = "Lời nhắn")]
        [StringLength(500, ErrorMessage = "Lời nhắn tối đa 500 ký tự")]
        public string? LoiNhan { get; set; }
    }
}
