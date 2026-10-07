// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Entity TaiKhoan phục vụ đăng nhập, Session và phân quyền.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models
{
    public static class VaiTroConst
    {
        public const string Admin = "Admin";
        public const string NhanVien = "NhanVien";
        public const string KhachHang = "KhachHang";
    }

    public static class TrangThaiDon
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DaXacNhan = "Đã xác nhận";
        public const string DangChuanBi = "Đang chuẩn bị";
        public const string SanSang = "Sẵn sàng";
        public const string DangGiao = "Đang giao";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";
    }

    public static class HinhThucNhanConst
    {
        public const string GiaoTanNoi = "Giao tận nơi";
        public const string KhachDenNhan = "Khách đến nhận";
    }

    public static class KieuSanPhamConst
    {
        public const string BoHoa = "Bó hoa";
        public const string GioHoa = "Giỏ hoa";
        public const string HopHoa = "Hộp hoa";
        public const string KeHoa = "Kệ hoa";
    }
}
