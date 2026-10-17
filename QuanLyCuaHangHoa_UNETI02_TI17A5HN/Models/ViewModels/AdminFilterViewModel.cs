// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Tham số tìm kiếm, lọc, sắp xếp dùng chung cho Admin.

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class AdminFilterViewModel
    {
        public string Area { get; set; } = "Admin";
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "Index";

        public string? TuKhoa { get; set; }
        public string? SapXep { get; set; }
        public bool? TrangThai { get; set; }

        public string Placeholder { get; set; } = "Nhập từ khóa tìm kiếm...";

        public bool HienThiTrangThai { get; set; } = true;

        public List<AdminSortOption> SapXepOptions { get; set; } = new();

        public string? VaiTro { get; set; }
        public bool HienThiVaiTro { get; set; } = false;
    }

    public class AdminSortOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
