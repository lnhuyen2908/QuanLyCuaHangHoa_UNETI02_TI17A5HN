// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: ViewModel Quản lý Loại hoa: hiển thị danh sách, đếm số giống hoa bằng LINQ, tìm kiếm, lọc và thống kê.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class LoaiHoaItemViewModel
    {
        public int MaLoaiHoa { get; set; }
        public string TenLoaiHoa { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; }
        // Số giống hoa trực thuộc đếm bằng LINQ
        public int SoGiongHoa { get; set; }
    }

    public class LoaiHoaIndexViewModel
    {
        public PagedList<LoaiHoaItemViewModel> DanhSach { get; set; } = null!;
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public int TongLoaiHoa { get; set; }
        public int TongDangKinhDoanh { get; set; }
        public int TongTamNgung { get; set; }
        public int TongSoGiongHoa { get; set; }
    }
}
