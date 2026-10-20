// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Ghép đơn, chi tiết, lịch sử, giao nhận và thanh toán để hiển thị Admin.
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

public class DonDatHoaDetailViewModel
{
    // Don đã Include các navigation cần thiết tại Controller; View không tự truy vấn DB.
    public DonDatHoa Don { get; set; } = null!;
    // Module 4 có thể ghi nhiều đợt thanh toán; cộng tất cả khoản đã ghi vào DB.
    public decimal DaThanhToan => Don.ThanhToans.Sum(t => t.SoTien);
    // Không hiển thị số âm nếu thanh toán đã đạt tổng tiền.
    public decimal ConLai => Math.Max(0, Don.TongTien - DaThanhToan);
    // Đơn chứa thiết kế riêng chưa báo giá thì Admin không được xác nhận.
    public bool ChuaChotGia => Don.ChiTietDonHoas.Any(c => c.MaMau == null && !c.DaChotGia);
}
