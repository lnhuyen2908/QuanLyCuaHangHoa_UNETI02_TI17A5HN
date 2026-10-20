
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - ViewModel khách hàng đánh giá đơn.

using System.ComponentModel.DataAnnotations;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    public class DanhGiaCreateViewModel
    {
        [Range(1, int.MaxValue)]
        public int MaDon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao.")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        [Display(Name = "Số sao đánh giá")]
        public int SoSao { get; set; }

        [StringLength(1000,
            ErrorMessage = "Nội dung tối đa 1000 ký tự.")]
        [Display(Name = "Nội dung đánh giá")]
        public string? NoiDung { get; set; }
    }
}
