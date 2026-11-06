using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class ThanhToanLichSuViewModel
    {
        public DonDatHoa DonHang { get; set; } = null!;

        public List<ThanhToanLichSuItemViewModel> GiaoDich
        { get; set; } = new();

        public decimal TongDaThanhToan { get; set; }

        public decimal SoTienConLai { get; set; }
    }

    public class ThanhToanLichSuItemViewModel
    {
        public int MaThanhToan { get; set; }

        public DateTime NgayThanhToan { get; set; }

        public decimal SoTien { get; set; }

        public string PhuongThuc { get; set; } = "";

        public string TenNguoiGhiNhan { get; set; } = "";

        public string? GhiChu { get; set; }
    }
}