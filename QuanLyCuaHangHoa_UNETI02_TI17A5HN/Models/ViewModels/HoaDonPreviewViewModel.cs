
using System;
using System.Collections.Generic;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels
{
    // Dữ liệu hóa đơn được lấy từ database, dùng chung cho trang xem trước và PDF.
    public class HoaDonPreviewViewModel
    {
        public int MaDon { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime ThoiGianNhanGiao { get; set; }

        public string TenKhachHang { get; set; } = "";
        public string NguoiNhan { get; set; } = "";
        public string SoDienThoaiNguoiNhan { get; set; } = "";
        public string HinhThucNhan { get; set; } = "";
        public string DiaChiGiao { get; set; } = "";
        public string LoiNhan { get; set; } = "";

        public decimal PhiGiaoHang { get; set; }
        public decimal PhuPhi { get; set; }
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConLai { get; set; }

        public List<HoaDonChiTietViewModel> ChiTiet { get; set; } = new();
        public List<HoaDonThanhToanViewModel> ThanhToans { get; set; } = new();
    }

    public class HoaDonChiTietViewModel
    {
        public string TenSanPham { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string YeuCauRieng { get; set; } = "";
    }

    public class HoaDonThanhToanViewModel
    {
        public DateTime NgayThanhToan { get; set; }
        public string PhuongThuc { get; set; } = "";
        public decimal SoTien { get; set; }
    }
}
