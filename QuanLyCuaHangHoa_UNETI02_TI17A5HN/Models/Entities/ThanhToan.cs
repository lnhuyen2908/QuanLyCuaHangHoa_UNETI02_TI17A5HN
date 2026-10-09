using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.NguyenThiDao_23103100245
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Quản lý thông tin và lịch sử thanh toán đơn hàng.

    public class ThanhToan
    {
        [Key]
        [Display(Name = "Mã thanh toán")]
        public int MaThanhToan { get; set; }

        [Required]
        [Display(Name = "Mã đơn")]
        public int MaDon { get; set; }

        [Required]
        [Display(Name = "Ngày thanh toán")]
        public DateTime NgayThanhToan { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Phương thức")]
        public string PhuongThuc { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Người ghi nhận")]
        public int NguoiGhiNhan { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation Property

        //public DonDatHoa DonDatHoa { get; set; } = null!;

        // ở class DonDatHoa
        //public ICollection<LichSuXuLyDon> LichSuXuLyDons { get; set; }
        //    = new List<LichSuXuLyDon>();
    }
}