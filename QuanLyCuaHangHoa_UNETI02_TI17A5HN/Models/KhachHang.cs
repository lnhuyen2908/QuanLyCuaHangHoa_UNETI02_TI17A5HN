using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng Entity KhachHang

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models
{
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255)]
        public string DiaChi { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;


        // KhachHang - TaiKhoan
        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan TaiKhoan { get; set; } = null!;


        // KhachHang 1 - n DonDatHoa
        public ICollection<DonDatHoa> DonDatHoas { get; set; } = new List<DonDatHoa>();
    }
}
