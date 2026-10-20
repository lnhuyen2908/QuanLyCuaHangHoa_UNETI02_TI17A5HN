// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Xây dựng ViewModel quản lý danh sách hoa tươi, lưu trữ thông tin tìm kiếm, lọc, sắp xếp, phân trang và danh sách dữ liệu cho các bộ lọc

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class HoaTuoiListViewModel
    {
        public PagedList<HoaTuoi> DanhSachHoa { get; set; } = new();

        public string? TuKhoa { get; set; }

        public int? LoaiHoaId { get; set; }

        public string? TrangThai { get; set; }

        public string? SapXep { get; set; }

        public int TrangHienTai { get; set; } = 1;

        public int SoBanGhiMoiTrang { get; set; } = 10;

        public string? MauSac { get; set; }

        public string? DonViTinh { get; set; }

        public IEnumerable<SelectListItem> DanhSachMauSac { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DanhSachDonViTinh { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> DanhSachLoaiHoa { get; set; } = new List<SelectListItem>();
    }
}
