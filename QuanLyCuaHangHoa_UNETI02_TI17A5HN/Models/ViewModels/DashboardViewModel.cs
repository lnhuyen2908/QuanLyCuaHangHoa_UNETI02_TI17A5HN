
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: ViewModel Dashboard thống kê Module 4.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê trạng thái đơn hàng
        public int TongDon { get; set; }
        public int ChoXacNhan { get; set; }
        public int DaXacNhan { get; set; }
        public int DangChuanBi { get; set; }
        public int SanSang { get; set; }
        public int DangGiao { get; set; }
        public int HoanThanh { get; set; }
        public int DaHuy { get; set; }

        // Doanh thu thực tế và giao nhận
        public decimal DoanhThuNgay { get; set; }
        public decimal DoanhThuThang { get; set; }
        public int GiaoHomNay { get; set; }
        public int DonConThieuTien { get; set; }

        // Thống kê yêu cầu thiết kế riêng chưa chốt giá.
        public int SoYeuCauChoGia { get; set; }
        public DashboardYeuCauThietKe? YeuCauMoiNhat
        { get; set; }

        // Danh sách và biểu đồ
        public List<DashboardDiemDoanhThu> DoanhThu7Ngay
        { get; set; } = new();

        public List<DashboardMauBanChay> MauBanChay
        { get; set; } = new();

        // Thống kê tỷ lệ tiêu thụ theo kiểu sản phẩm.
        public List<DashboardThongKeKieu> ThongKeKieuSanPham
        { get; set; } = new();

        public List<DashboardDonCanXuLy> DonCanXuLy
        { get; set; } = new();
    }

    public class DashboardDiemDoanhThu
    {
        public DateTime Ngay { get; set; }
        public decimal SoTien { get; set; }
    }

    public class DashboardMauBanChay
    {
        public int MaMau { get; set; }
        public string TenMau { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class DashboardDonCanXuLy
    {
        public int MaDon { get; set; }
        public string NguoiNhan { get; set; } = "";
        public DateTime ThoiGianNhanGiao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "";
        // Tên mẫu hoa đầu tiên trong đơn đặt hàng.
        public string TenMau { get; set; } = "";
    }

    // Dữ liệu thống kê kiểu sản phẩm cho Dashboard.
    public class DashboardThongKeKieu
    {
        public string TenKieu { get; set; } = "";
        public int SoLuong { get; set; }
        public double TyLe { get; set; }
    }

    // ViewModel yêu cầu thiết kế riêng trên Dashboard.
    public class DashboardYeuCauThietKe
    {
        public int MaChiTiet { get; set; }
        public int MaDon { get; set; }
        public string TenKhach { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public DateTime NgayDat { get; set; }
    }
}
