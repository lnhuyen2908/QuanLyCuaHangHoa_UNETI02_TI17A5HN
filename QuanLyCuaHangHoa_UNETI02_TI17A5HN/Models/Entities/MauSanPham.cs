// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Xây dựng Entity Hoa tươi, Mẫu sản phẩm, Tra cứu

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class MauSanPham
    {
        [Key]
        public int MaMau { get; set; }

        [Required]
        [StringLength(150)]
        public string TenMau { get; set; } = string.Empty;

        [Required]
        public int MaDip { get; set; }

        [Required]
        [StringLength(30)]
        public string KieuSanPham { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaCoBan { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        [StringLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual DipSuDung DipSuDung { get; set; } = null!;

        public virtual ICollection<ChiTietMau> ChiTietMaus { get; set; } = new List<ChiTietMau>();
    }
}
