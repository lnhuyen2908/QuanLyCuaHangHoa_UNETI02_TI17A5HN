// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Lưu dữ liệu phục vụ thêm và sửa mẫu sản phẩm

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class MauSanPhamFormViewModel
    {
        public int MaMau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mẫu.")]
        [StringLength(150)]
        [Display(Name = "Tên mẫu")]
        public string TenMau { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn dịp sử dụng.")]
        [Display(Name = "Dịp sử dụng")]
        public int MaDip { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn kiểu sản phẩm.")]
        [StringLength(30)]
        public string KieuSanPham { get; set; } = "Bó hoa";

        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        public decimal GiaCoBan { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        public IFormFile? TepAnh { get; set; }

        [StringLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public List<ChiTietMauFormItem> ChiTietMaus { get; set; } = new();
        public IEnumerable<SelectListItem> DanhSachDip { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DanhSachHoa { get; set; } = new List<SelectListItem>();
    }
    public class ChiTietMauFormItem
    {
        public int? MaChiTietMau { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn hoa thành phần.")]
        public int MaHoa { get; set; }

        [Range(1, 9999, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int SoLuongTieuChuan { get; set; } = 1;

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }

}
