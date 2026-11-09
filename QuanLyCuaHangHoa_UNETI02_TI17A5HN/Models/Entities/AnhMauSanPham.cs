// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Entity lưu nhiều ảnh cho mẫu sản phẩm

using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    public class AnhMauSanPham
    {
        [Key]
        public int MaAnh { get; set; }

        [Required]
        public int MaMau { get; set; }

        [Required]
        [StringLength(255)]
        public string DuongDan { get; set; } = string.Empty;

        public int ThuTu { get; set; }

        public virtual MauSanPham MauSanPham { get; set; } = null!;
    }
}
