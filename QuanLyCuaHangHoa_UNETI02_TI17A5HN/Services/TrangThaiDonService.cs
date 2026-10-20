
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Kiểm tra luồng chuyển trạng thái đơn hàng.

    public class TrangThaiDonService
    {
        public bool CoTheChuyenTrangThai(
            string? trangThaiCu,
            string? trangThaiMoi,
            string? hinhThucNhan)
        {
            if (string.IsNullOrWhiteSpace(trangThaiCu) ||
                string.IsNullOrWhiteSpace(trangThaiMoi) ||
                string.IsNullOrWhiteSpace(hinhThucNhan))
            {
                return false;
            }

            if (trangThaiCu == trangThaiMoi)
            {
                return false;
            }

            return trangThaiCu switch
            {
                TrangThaiDon.ChoXacNhan =>
                    trangThaiMoi == TrangThaiDon.DaXacNhan ||
                    trangThaiMoi == TrangThaiDon.DaHuy,

                TrangThaiDon.DaXacNhan =>
                    trangThaiMoi == TrangThaiDon.DangChuanBi,

                TrangThaiDon.DangChuanBi =>
                    trangThaiMoi == TrangThaiDon.SanSang,

                TrangThaiDon.SanSang =>
                    (hinhThucNhan == HinhThucNhanConst.GiaoTanNoi &&
                     trangThaiMoi == TrangThaiDon.DangGiao) ||
                    (hinhThucNhan == HinhThucNhanConst.KhachDenNhan &&
                     trangThaiMoi == TrangThaiDon.HoanThanh),

                TrangThaiDon.DangGiao =>
                    hinhThucNhan == HinhThucNhanConst.GiaoTanNoi &&
                    trangThaiMoi == TrangThaiDon.HoanThanh,

                TrangThaiDon.HoanThanh => false,

                TrangThaiDon.DaHuy => false,

                _ => false
            };
        }
    }
}
