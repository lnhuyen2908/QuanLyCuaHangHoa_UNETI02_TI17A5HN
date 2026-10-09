using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Quản lý thông tin giao nhận đơn đặt hoa.

    public class GiaoNhan
    {
        [Key]
        [Display(Name = "Mã giao nhận")]
        public int MaGiaoNhan { get; set; }

        [Required]
        [Display(Name = "Mã đơn")]
        public int MaDon { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Hình thức")]
        public string HinhThuc { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Thời gian dự kiến")]
        public DateTime ThoiGianDuKien { get; set; }

        [Display(Name = "Thời gian thực tế")]
        public DateTime? ThoiGianThucTe { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(100)]
        [Display(Name = "Người giao")]
        public string? NguoiGiao { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Property
        public DonDatHoa DonDatHoa { get; set; } = null!;
    }
}