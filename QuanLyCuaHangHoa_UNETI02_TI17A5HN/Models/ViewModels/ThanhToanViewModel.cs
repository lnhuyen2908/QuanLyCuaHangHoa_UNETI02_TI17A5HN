
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel quản lý thanh toán.

using System.ComponentModel.DataAnnotations;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class ThanhToanViewModel
    {
        public int? MaDon { get; set; }

        public decimal TongTien { get; set; }
        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai =>
            Math.Max(0, TongTien - TongDaThanhToan);

        public List<ThanhToan> LichSu { get; set; } = new();

        public Dictionary<int, string> TenNhanVien { get; set; }
            = new();

        public List<DonDatHoa> DonHang { get; set; }
            = new();
    }

    public class ThanhToanCreateViewModel
    {
        [Range(1, int.MaxValue)]
        public int MaDon { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999",
            ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức.")]
        [StringLength(30)]
        public string PhuongThuc { get; set; } = "";

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public decimal TongTien { get; set; }
        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai =>
            Math.Max(0, TongTien - TongDaThanhToan);
    }
}
