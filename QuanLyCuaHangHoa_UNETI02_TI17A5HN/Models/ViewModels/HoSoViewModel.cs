using System.ComponentModel.DataAnnotations;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class HoSoViewModel
    {
        public int MaTaiKhoan { get; set; }
        public int? MaKhachHang { get; set; }

        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        public string VaiTro { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = true;

        // Danh sách toàn bộ đơn hàng
        public List<DonDatHoa> DanhSachDonHang { get; set; } = new();

        // Đơn hàng gần nhất đang xử lý hoặc vừa đặt
        public DonDatHoa? DonHangGanNhat { get; set; }

        // Danh sách đơn hoa thiết kế riêng
        public List<DonDatHoa> DonHangThietKeRieng { get; set; } = new();

        // Thuộc tính tiện ích
        public int TongSoDonHang => DanhSachDonHang.Count;
        public int SoDonHoanThanh => DanhSachDonHang.Count(d => d.TrangThai == TrangThaiDon.HoanThanh);
        public decimal TongChiTieu => DanhSachDonHang.Where(d => d.TrangThai == TrangThaiDon.HoanThanh).Sum(d => d.TongTien);

        public string HangThanhVien
        {
            get
            {
                if (TongChiTieu >= 10_000_000) return "Diamond VIP";
                if (TongChiTieu >= 5_000_000) return "Gold VIP";
                if (TongChiTieu >= 2_000_000) return "Silver VIP";
                return "Hội Viên Thân Thiết";
            }
        }
    }

    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }
}
