
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: ViewModel thống kê LINQ nâng cao - Module 4.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class ThongKeViewModel
    {
        public List<ThongKeDoanhThuThang> DoanhThuTheoThang
        { get; set; } = new();

        public List<ThongKeSoLuong> DonTheoTrangThai
        { get; set; } = new();

        public List<ThongKeSoTien> GiaTriTheoKieu
        { get; set; } = new();

        public List<ThongKeSoTien> GiaTriTheoDip
        { get; set; } = new();

        public List<ThongKeSoLuong> MauBanChay
        { get; set; } = new();

        public List<ThongKeSoLuong> LoaiHoaPhoBien
        { get; set; } = new();

        public List<ThongKeSoLuong> KhachHangNhieuDon
        { get; set; } = new();

        public decimal GiaTriDonTrungBinh { get; set; }

        public double DiemDanhGiaTrungBinh { get; set; }

        public int TongDanhGia { get; set; }
    }

    public class ThongKeDoanhThuThang
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public decimal SoTien { get; set; }
    }

    public class ThongKeSoLuong
    {
        public string Ten { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class ThongKeSoTien
    {
        public string Ten { get; set; } = "";
        public decimal SoTien { get; set; }
    }
}
