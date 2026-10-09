// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Entity TaiKhoan phục vụ đăng nhập, Session và phân quyền.

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Tên loại hoa không trùng
    [Index(nameof(TenLoaiHoa), IsUnique = true)]
    public class LoaiHoa
    {
        [Key]
        public int MaLoaiHoa { get; set; }

        [Required(ErrorMessage = "Tên loại hoa là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên loại hoa tối đa 100 ký tự")]
        [Display(Name = "Tên loại hoa")]
        public string TenLoaiHoa { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // true = Hoạt động, false = Ngừng hoạt động (không dùng cho sản phẩm mới)
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation Property: LoaiHoa (1) - (n) HoaTuoi (Module 2)
        public virtual ICollection<HoaTuoi> HoaTuois { get; set; } = new List<HoaTuoi>();
    }
}