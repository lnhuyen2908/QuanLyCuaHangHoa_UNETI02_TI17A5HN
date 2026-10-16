// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: ViewModel Quản lý Dịp sử dụng: hiển thị danh sách, đếm số mẫu hoa bằng LINQ, tìm kiếm, lọc và thống kê.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DipSuDungItemViewModel
    {
        public int MaDip { get; set; }
        public string TenDip { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; }
        // Số mẫu sản phẩm liên kết được đếm bằng LINQ
        public int SoMauHoa { get; set; }
    }

    public class DipSuDungIndexViewModel
    {
        public PagedList<DipSuDungItemViewModel> DanhSach { get; set; } = null!;
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public int TongSoDip { get; set; }
        public int TongDangHoatDong { get; set; }
        public int TongNgungHoatDong { get; set; }
        public int TongMauHoaLienKet { get; set; }
    }
}
