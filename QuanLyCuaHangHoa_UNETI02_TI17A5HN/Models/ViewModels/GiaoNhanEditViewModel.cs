
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel sửa phiếu giao nhận.

using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class GiaoNhanEditViewModel
    {
        public int MaGiaoNhan { get; set; }

        public int MaDon { get; set; }

        public string TenNguoiNhan { get; set; } = "";

        public string HinhThuc { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn thời gian dự kiến.")]
        [Display(Name = "Thời gian dự kiến")]
        public DateTime ThoiGianDuKien { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ giao")]
        public string? DiaChi { get; set; }

        [StringLength(100)]
        [Display(Name = "Người giao")]
        public string? NguoiGiao { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}
