
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel thêm phiếu giao nhận.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class GiaoNhanCreateViewModel
    {
        [Display(Name = "Đơn đặt hoa")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn đơn đặt hoa.")]
        public int MaDon { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian dự kiến.")]
        [Display(Name = "Thời gian dự kiến")]
        public DateTime ThoiGianDuKien { get; set; }
            = DateTime.Now.AddHours(2);

        [StringLength(250)]
        [Display(Name = "Địa chỉ giao")]
        public string? DiaChi { get; set; }

        [StringLength(100)]
        [Display(Name = "Người giao")]
        public string? NguoiGiao { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public List<SelectListItem> DonHopLe { get; set; }
            = new();
    }
}
