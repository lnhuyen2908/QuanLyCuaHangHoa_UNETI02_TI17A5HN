// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Lưu dữ liệu danh sách và điều kiện lọc mẫu sản phẩm.

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class MauSanPhamListViewModel
    {
        public PagedList<MauSanPham> DanhSachMau { get; set; } = new();
        public string? TuKhoa { get; set; }
        public int? MaDip { get; set; }
        public string? KieuSanPham { get; set; }
        public string? MauSac { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string? TrangThai { get; set; }
        public string? SapXep { get; set; }
        public int TrangHienTai { get; set; } = 1;
        public int SoBanGhiMoiTrang { get; set; } = 12;
        public IEnumerable<SelectListItem> DanhSachDip { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DanhSachKieu { get; set; } = new List<SelectListItem>();
    }
}
