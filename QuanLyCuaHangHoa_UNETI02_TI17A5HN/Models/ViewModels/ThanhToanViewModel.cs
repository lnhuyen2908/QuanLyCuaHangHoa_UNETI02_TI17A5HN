// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel quản lý thanh toán và lựa chọn đơn hàng.

using System.ComponentModel.DataAnnotations;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    // Dữ liệu một đơn hàng trong bảng lựa chọn đơn cần ghi nhận thanh toán.
    public class ThanhToanDonHangViewModel
    {
        public int MaDon { get; set; }
        public string NguoiNhan { get; set; } = string.Empty;
        public string SoDienThoaiNguoiNhan { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public string TrangThaiDon { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai =>
            Math.Max(0m, TongTien - TongDaThanhToan);
    }

    public class ThanhToanViewModel
    {
        // Bộ lọc danh sách đơn hàng.
        public string? TuKhoa { get; set; }
        public string? TinhTrangThanhToan { get; set; }
        public int? MaDon { get; set; }

        public PagedList<ThanhToanDonHangViewModel> DanhSachDonHang { get; set; }
            = new();

        // Thông tin và lịch sử của đơn đang được chọn.
        public decimal TongTien { get; set; }
        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai =>
            Math.Max(0m, TongTien - TongDaThanhToan);

        public List<ThanhToan> LichSu { get; set; } = new();

        public Dictionary<int, string> TenNhanVien { get; set; }
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
        public string PhuongThuc { get; set; } = PhuongThucThanhToanConst.TienMat;

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public decimal TongTien { get; set; }
        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai =>
            Math.Max(0m, TongTien - TongDaThanhToan);
    }
}
