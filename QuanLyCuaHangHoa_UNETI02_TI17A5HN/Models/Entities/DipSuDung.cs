// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Entity TaiKhoan phục vụ đăng nhập, Session và phân quyền.


using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models
{
    // Tên dịp sử dụng không trùng
    [Index(nameof(TenDip), IsUnique = true)]
    public class DipSuDung
    {
        [Key]
        public int MaDip { get; set; }

        [Required(ErrorMessage = "Tên dịp sử dụng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên dịp tối đa 100 ký tự")]
        [Display(Name = "Tên dịp sử dụng")]
        public string TenDip { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // true = Hoạt động, false = Ngừng hoạt động (không dùng cho sản phẩm mới)
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation Property: DipSuDung (1) - (n) MauSanPham (Module 2)
        public virtual ICollection<MauSanPham> MauSanPhams { get; set; } = new List<MauSanPham>();
    }
}
