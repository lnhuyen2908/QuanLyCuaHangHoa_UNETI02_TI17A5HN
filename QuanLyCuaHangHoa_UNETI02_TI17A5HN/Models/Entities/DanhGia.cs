using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Quản lý đánh giá của khách hàng đối với đơn hàng đã hoàn thành.

    public class DanhGia
    {
        [Key]
        [Display(Name = "Mã đánh giá")]
        public int MaDanhGia { get; set; }

        [Required]
        [Display(Name = "Mã đơn")]
        public int MaDon { get; set; }

        [Required]
        [Display(Name = "Mã khách hàng")]
        public int MaKhachHang { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        [Required]
        [Display(Name = "Ngày đánh giá")]
        public DateTime NgayDanhGia { get; set; }

        // Navigation Property
        public DonDatHoa DonDatHoa { get; set; } = null!;

        public KhachHang KhachHang { get; set; } = null!;
    }
}