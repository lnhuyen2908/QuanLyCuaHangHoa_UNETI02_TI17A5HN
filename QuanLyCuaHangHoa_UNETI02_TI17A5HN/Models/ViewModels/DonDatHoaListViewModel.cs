// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Dữ liệu lọc/phân trang danh sách đơn hàng Admin, tổng hợp trạng thái từ DB.
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

public class DonDatHoaListViewModel
{
    // Bộ lọc tìm kiếm được giữ lại khi chuyển trang.
    public string? TuKhoa { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    // Controller tính TongTrang từ số kết quả sau lọc và lấy tối đa 10 bản ghi/trang.
    public int Trang { get; set; } = 1;
    public int TongTrang { get; set; } = 1;
    public int TongBanGhi { get; set; }
    // Số liệu đầu trang được tính trực tiếp từ DB, độc lập với danh sách đã lọc.
    public int DonHomNay { get; set; }
    public decimal DoanhSoHomNay { get; set; }
    public int ChoXacNhan { get; set; }
    public int DangChuanBi { get; set; }
    public int SanSang { get; set; }
    public int DangGiao { get; set; }
    public int HoanThanhThang { get; set; }
    // Các đơn đã được truy vấn và phân trang ở Controller.
    public List<DonDatHoa> DanhSach { get; set; } = new();
}
