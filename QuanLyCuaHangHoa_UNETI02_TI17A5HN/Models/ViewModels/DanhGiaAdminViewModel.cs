
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel quản lý đánh giá Admin.

using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DanhGiaAdminViewModel
    {
        public string? TuKhoa { get; set; }

        public string SapXep { get; set; } = "date_new";

        public int TongDanhGia { get; set; }

        public double DiemTrungBinh { get; set; }

        public double TyLe5Sao { get; set; }

        // Chỉ số mảng là số sao, từ 1 đến 5.
        public int[] ThongKeSao { get; set; } = new int[6];

        public PagedList<DanhGia> DanhSach { get; set; }
            = new();
    }
}
