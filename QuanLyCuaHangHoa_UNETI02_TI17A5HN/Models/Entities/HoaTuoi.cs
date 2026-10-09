// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Xây dựng Entity Hoa tươi, Mẫu sản phẩm, Tra cứu

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class HoaTuoi
    {
        [Key]
        public int MaHoa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenHoa { get; set; } = string.Empty;

        [Required]
        public int MaLoaiHoa { get; set; }

        [Required]
        [StringLength(50)]
        public string MauSac { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string DonViTinh { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaThamKhao { get; set; }

        [StringLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation Property
        public virtual LoaiHoa LoaiHoa { get; set; } = null!;

        public virtual ICollection<ChiTietMau> ChiTietMaus { get; set; } = new List<ChiTietMau>();
    }
}
