// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng DonHangService để xử lý nghiệp vụ tính tiền

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
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

        // Phí giao tận nơi áp dụng khi tạo đơn.
        public const decimal PhiGiaoTanNoi = 30_000m;

        // Khách đến nhận không mất phí giao hàng.
        public static decimal TinhPhiGiaoHang(string hinhThucNhan)
        {
            return hinhThucNhan == HinhThucNhanConst.GiaoTanNoi
                ? PhiGiaoTanNoi
                : 0m;
        }
    }
}
