// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Truyền danh sách khách hàng, bộ lọc và thống kê sang View.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class KhachHangIndexViewModel
    {
        // Dùng PagedList<T>
        public PagedList<KhachHang> DanhSach { get; set; } = new();
        public string? TuKhoa { get; set; }
        public bool? TrangThai { get; set; }
        public string SapXep { get; set; } = "name_asc";
        public int TongKhachHang { get; set; }
        public int TongDangHoatDong { get; set; }
        public int TongBiKhoa { get; set; }
    }
}
