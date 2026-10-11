// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng Entity KhachHang

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities
{
    // Mỗi tài khoản chỉ tương ứng với một khách hàng
    [Index(nameof(MaTaiKhoan), IsUnique = true)]
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Tài khoản là bắt buộc")]
        [Display(Name = "Mã tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên khách hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // true = Hoạt động, false = Ngừng hoạt động
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation Property: KhachHang (1) - (1) TaiKhoan
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;

        // Navigation Property: KhachHang (1) - (n) DonDatHoa
        public virtual ICollection<DonDatHoa> DonDatHoas { get; set; } = new List<DonDatHoa>();
    }
}
