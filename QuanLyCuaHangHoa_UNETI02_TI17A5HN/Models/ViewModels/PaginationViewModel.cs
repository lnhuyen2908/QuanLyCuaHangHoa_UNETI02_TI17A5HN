// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Dữ liệu hiển thị phân trang dùng chung cho Admin.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int TrangHienTai { get; set; } = 1;
        public int TongSoTrang { get; set; }
        public int TongSoBanGhi { get; set; }
        public int SoBanGhiMoiTrang { get; set; } = 10;
        public string Area { get; set; } = "Admin";
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "Index";
        public Dictionary<string, string?> BoLoc { get; set; } = new();
    }
}
