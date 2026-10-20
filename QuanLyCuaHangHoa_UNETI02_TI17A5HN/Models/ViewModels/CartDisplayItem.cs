// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Dữ liệu hiển thị giỏ Session kết hợp tên/giá/ảnh từ database.
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

public class CartDisplayItem
{
    // Mã và số lượng lấy từ Session; thông tin tên/ảnh/giá được service bổ sung từ DB.
    public int MaMau { get; set; }
    public int SoLuong { get; set; }
    public string GiayGoiRuyBang { get; set; } = "";
    public string LoiChuc { get; set; } = "";
    public string TenMau { get; set; } = "";
    public string? HinhAnh { get; set; }
    public decimal DonGia { get; set; }
    // False nếu mẫu bị xóa hoặc đã ngừng bán; dùng để khóa đặt hàng ở View.
    public bool ConBan { get; set; }
    // Thành tiền chỉ là giá trị hiển thị của một dòng giỏ, chưa phải số tiền đã thanh toán.
    public decimal ThanhTien => SoLuong * DonGia;
}
