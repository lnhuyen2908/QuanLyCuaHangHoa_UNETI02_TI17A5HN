// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng DonHangService để xử lý nghiệp vụ tính tiền
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    public class DonHangService
    {
        public decimal TinhThanhTien(int soLuong, decimal donGia)
        {
            if (soLuong <= 0 || donGia < 0)
                throw new ArgumentException("Dữ liệu không hợp lệ");

            return soLuong * donGia;
        }

        public decimal TinhTongTien(
            IEnumerable<decimal> thanhTiens,
            decimal phiGiaoHang,
            decimal phuPhi)
        {
            if (phiGiaoHang < 0 || phuPhi < 0)
                throw new ArgumentException("Phí không hợp lệ");

            return thanhTiens.Sum() + phiGiaoHang + phuPhi;
        }
    }
}
