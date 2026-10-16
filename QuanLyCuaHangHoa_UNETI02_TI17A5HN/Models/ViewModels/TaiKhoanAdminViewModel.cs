// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: ViewModel phục vụ tìm kiếm, lọc, thống kê và form thêm/sửa tài khoản cho khu vực Admin.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    // Dùng cho trang Index (Danh sách, tìm kiếm, lọc, thống kê)
    public class TaiKhoanAdminIndexViewModel
    {
        public PagedList<TaiKhoan> DanhSach { get; set; } = null!;
        public string? TuKhoa { get; set; }
        public string? VaiTro { get; set; }
        public bool? TrangThai { get; set; }
        public int TongTaiKhoan { get; set; }
        public int TongAdmin { get; set; }
        public int TongNhanVien { get; set; }
        public int TongKhachHang { get; set; }
    }

    // Alias để tương thích cả 2 cách đặt tên
    public class TaiKhoanAdminViewModel : TaiKhoanAdminIndexViewModel
    {
    }

    // Dùng cho Form Thêm mới 
    public class TaiKhoanCreateViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập từ 4-50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ, số và dấu gạch dưới")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }

    // Dùng cho Form Chỉnh sửa
    public class TaiKhoanEditViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty; // Chỉ đọc

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string VaiTro { get; set; } = string.Empty;

        public string? MatKhauMoi { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}
