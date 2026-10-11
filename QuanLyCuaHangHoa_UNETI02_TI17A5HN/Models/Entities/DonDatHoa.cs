// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng Entity DonDatHoa

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class DonDatHoa
    {
        [Key]
        public int MaDon { get; set; }

        [Required(ErrorMessage = "Khách hàng là bắt buộc")]
        [Display(Name = "Mã khách hàng")]
        public int MaKhachHang { get; set; }

        // Thời gian hệ thống ghi nhận đơn
        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Hình thức nhận là bắt buộc")]
        [StringLength(30, ErrorMessage = "Hình thức nhận tối đa 30 ký tự")]
        [Display(Name = "Hình thức nhận")]
        public string HinhThucNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian nhận/giao là bắt buộc")]
        [Display(Name = "Thời gian nhận/giao")]
        public DateTime ThoiGianNhanGiao { get; set; }

        // Có thể null khi khách chọn nhận tại cửa hàng
        [StringLength(250, ErrorMessage = "Địa chỉ giao tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ giao")]
        public string? DiaChiGiao { get; set; }

        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
        [Display(Name = "Người nhận")]
        public string NguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại người nhận là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại người nhận không hợp lệ")]
        [Display(Name = "Số điện thoại người nhận")]
        public string SoDienThoaiNguoiNhan { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Lời nhắn tối đa 500 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Lời nhắn")]
        public string? LoiNhan { get; set; }

        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc")]
        [StringLength(30, ErrorMessage = "Trạng thái tối đa 30 ký tự")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Phí giao hàng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Phí giao hàng")]
        public decimal PhiGiaoHang { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Phụ phí phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Phụ phí")]
        public decimal PhuPhi { get; set; } = 0;

        [StringLength(250, ErrorMessage = "Lý do phụ phí tối đa 250 ký tự")]
        [Display(Name = "Lý do phụ phí")]
        public string? LyDoPhuPhi { get; set; }

        // Tổng tiền do hệ thống tính, không nhận trực tiếp từ form
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [StringLength(500, ErrorMessage = "Lý do hủy tối đa 500 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Lý do hủy")]
        public string? LyDoHuy { get; set; }

        // Navigation Property: KhachHang (1) - (n) DonDatHoa
        public virtual KhachHang KhachHang { get; set; } = null!;

        // Navigation Property: DonDatHoa (1) - (n) ChiTietDonHoa
        public virtual ICollection<ChiTietDonHoa> ChiTietDonHoas { get; set; } = new List<ChiTietDonHoa>();
    }
}
