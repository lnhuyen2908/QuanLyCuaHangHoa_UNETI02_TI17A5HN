// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Kiểm tra yêu cầu thiết kế hoa riêng, chưa thu tiền/chốt giá khi khách gửi.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

public class ThietKeRiengViewModel
{
    // Nhóm yêu cầu thiết kế: kiểu dáng, tông màu, loài hoa và ngân sách dự kiến.
    [Required(ErrorMessage = "Chọn kiểu sản phẩm"), StringLength(100)]
    public string LoaiSanPham { get; set; } = "";
    [Required(ErrorMessage = "Chọn tông màu"), StringLength(100)]
    public string TongMau { get; set; } = "";

    // Ảnh tùy chọn: Controller giới hạn 5MB và kiểm tra chữ ký file trước khi lưu.
    public IFormFile? AnhThamKhao { get; set; }
    [StringLength(300)] public string? LoaiHoa { get; set; }

    // Danh sách loại hoa đang hoạt động, lấy từ database để hiển thị.
    public List<LoaiHoa> DanhSachLoaiHoa { get; set; } = new();

    // Danh sách mã loại hoa mà khách đã chọn.
    public List<int> MaLoaiHoaDaChon { get; set; } = new();

    [Required(ErrorMessage = "Nhập ngân sách"), StringLength(100)]
    public string NganSach { get; set; } = "";
    [StringLength(200)]
    public string? NoiDungThiep { get; set; }
    [StringLength(350)]
    public string? GhiChu { get; set; }
    // Thông tin nhận hàng đi kèm đơn ngay cả khi chưa có báo giá.
    [Required(ErrorMessage = "Nhập tên người nhận"), StringLength(100)]
    public string NguoiNhan { get; set; } = "";
    [Required(ErrorMessage = "Nhập số điện thoại"), StringLength(15), Phone]
    public string SoDienThoai { get; set; } = "";
    [Required(ErrorMessage = "Chọn thời gian giao/nhận")]
    public DateTime? ThoiGianNhanGiao { get; set; }
    [Required]
    public string HinhThucNhan { get; set; } = "Giao tận nơi";
    // Chỉ bắt buộc địa chỉ khi chọn hình thức giao tận nơi (kiểm tra tại Controller).
    [StringLength(250)]
    public string? DiaChiGiao { get; set; }
}
