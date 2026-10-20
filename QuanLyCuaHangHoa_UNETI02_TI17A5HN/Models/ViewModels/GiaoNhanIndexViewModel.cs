
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel danh sách giao nhận.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class GiaoNhanIndexViewModel
    {
        public string? TuKhoa { get; set; }
        public string? TrangThaiGiaoNhan { get; set; }
        public string SapXep { get; set; } = "date_new";

        public PagedList<GiaoNhan> DanhSach { get; set; } = new();

        public int TongGiaoNhan { get; set; }
        public int ChoXuLy { get; set; }
        public int DangGiao { get; set; }
        public int HoanThanh { get; set; }
    }
}
