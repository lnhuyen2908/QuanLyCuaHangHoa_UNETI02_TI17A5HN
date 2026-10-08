// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng Entity ChiTietDonHoa

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class ChiTietDonHoa
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Required(ErrorMessage = "Đơn đặt hoa là bắt buộc")]
        [Display(Name = "Mã đơn")]
        public int MaDon { get; set; }

        // Có thể null nếu khách hàng đặt thiết kế riêng
        [Display(Name = "Mã mẫu")]
        public int? MaMau { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 1")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        // Đơn giá được chốt tại thời điểm đặt hàng
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [StringLength(1000, ErrorMessage = "Yêu cầu riêng tối đa 1000 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Yêu cầu riêng")]
        public string? YeuCauRieng { get; set; }

        // Thành tiền do hệ thống tính: SoLuong * DonGia
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Thành tiền phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; }

        // Dùng cho đơn thiết kế riêng:
        // true = Đã chốt giá, false = Chưa chốt giá
        [Display(Name = "Đã chốt giá")]
        public bool DaChotGia { get; set; }

        // Navigation Property: DonDatHoa (1) - (n) ChiTietDonHoa
        public virtual DonDatHoa DonDatHoa { get; set; } = null!;

        // Navigation Property: MauSanPham (1) - (n) ChiTietDonHoa
        // Có thể null khi khách đặt thiết kế riêng
        public virtual MauSanPham? MauSanPham { get; set; }
    }
}
