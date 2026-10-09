using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Xử lý trạng thái đơn và lưu lịch sử xử lý đơn.

    public class LichSuXuLyDon
    {
        [Key]
        [Display(Name = "Mã lịch sử")]
        public int MaLichSu { get; set; }

        [Required]
        [Display(Name = "Mã đơn")]
        public int MaDon { get; set; }

        [StringLength(30)]
        [Display(Name = "Trạng thái cũ")]
        public string? TrangThaiCu { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái mới")]
        public string TrangThaiMoi { get; set; } = string.Empty;

        [Display(Name = "Thời gian")]
        public DateTime ThoiGian { get; set; }

        [Required]
        [Display(Name = "Người thực hiện")]
        public int NguoiThucHien { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Property
        public DonDatHoa DonDatHoa { get; set; } = null!;
    }
}