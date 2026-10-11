// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Entity TaiKhoan phục vụ đăng nhập, Session và phân quyền.

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Unique Index đảm bảo tên đăng nhập không trùng ở mức cơ sở dữ liệu
    [Index(nameof(TenDangNhap), IsUnique = true)]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập từ 4 đến 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Giá trị lấy từ VaiTroConst: Admin / NhanVien / KhachHang
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        // true = Hoạt động, false = Bị khóa (tài khoản khóa không được đăng nhập)
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation Property: quan hệ 1-0..1 với KhachHang (Module 3)
        public virtual KhachHang? KhachHang { get; set; }
    }
}