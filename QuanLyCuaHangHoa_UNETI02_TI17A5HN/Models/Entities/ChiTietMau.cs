// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Xây dựng Entity Hoa tươi, Mẫu sản phẩm, Tra cứu

using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class ChiTietMau
    {
        [Key]
        public int MaChiTietMau { get; set; }

        [Required]
        public int MaMau { get; set; }

        [Required]
        public int MaHoa { get; set; }

        [Range(1, int.MaxValue)]
        public int SoLuongTieuChuan { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Navigation Property
        public virtual MauSanPham MauSanPham { get; set; } = null!;

        public virtual HoaTuoi HoaTuoi { get; set; } = null!;
    }
}
