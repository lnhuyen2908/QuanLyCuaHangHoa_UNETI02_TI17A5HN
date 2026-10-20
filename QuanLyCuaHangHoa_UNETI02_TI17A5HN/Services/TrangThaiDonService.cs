
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    // Họ và tên: Nguyễn Thị Đào
    // Mã sinh viên: 23103100245
    // Nội dung thực hiện: Kiểm tra chuyển trạng thái đơn,
    // cập nhật CSDL và lưu lịch sử xử lý đơn hàng.

    public class TrangThaiDonService
    {
        // Nội dung bổ sung tuần 5: Kết nối CSDL để xử lý đơn.

        private readonly AppDbContext _db;
        public TrangThaiDonService(AppDbContext db)
        {
            _db = db;   //_db cho phép Service truy vấn và cập nhật các bảng DonDatHoas, GiaoNhans, ThanhToans và LichSuXuLyDons.
        }

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

        // Chuyển trạng thái đơn và lưu lịch sử vào SQL Server.
        public async Task<(bool ThanhCong, string ThongBao)> ChuyenAsync(
            int maDon,
            string trangThaiMoi,
            int nguoiThucHien,
            string? ghiChu)
        {
            if (nguoiThucHien <= 0)
                return (false, "Người thực hiện không hợp lệ.");

            if (ghiChu?.Length > 500)
                return (false, "Ghi chú tối đa 500 ký tự.");

            await using var tx = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            var don = await _db.DonDatHoas.AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (don == null)
                return (false, "Không tìm thấy đơn hàng.");

            // Xác nhận và hủy thuộc phần của Hương.
            // Module 4 chỉ xử lý đơn đã xác nhận.
            if (don.TrangThai == TrangThaiDon.ChoXacNhan ||
                don.TrangThai == TrangThaiDon.DaHuy ||
                don.TrangThai == TrangThaiDon.HoanThanh)
            {
                return (false, "Đơn không thuộc bước xử lý Module 4.");
            }

            // Tái sử dụng hàm đã viết từ tuần 3.
            if (!CoTheChuyenTrangThai(
                don.TrangThai,
                trangThaiMoi,
                don.HinhThucNhan))
            {
                return (false, "Không được chuyển trạng thái trái quy trình.");
            }

            var giaoNhan = await _db.GiaoNhans
                .FirstOrDefaultAsync(g => g.MaDon == maDon);

            // Kiểm tra điều kiện khi bắt đầu giao.
            if (trangThaiMoi == TrangThaiDon.DangGiao)
            {
                if (giaoNhan == null ||
                    giaoNhan.HinhThuc != HinhThucNhanConst.GiaoTanNoi ||
                    giaoNhan.ThoiGianDuKien < don.NgayDat.AddMinutes(30) ||
                    string.IsNullOrWhiteSpace(giaoNhan.DiaChi) ||
                    string.IsNullOrWhiteSpace(don.NguoiNhan) ||
                    string.IsNullOrWhiteSpace(don.SoDienThoaiNguoiNhan))
                {
                    return (false, "Thông tin giao nhận chưa hợp lệ.");
                }

                giaoNhan.TrangThai = TrangThaiGiaoNhanConst.DangGiao;
            }

            // Kiểm tra điều kiện hoàn thành đơn.
            if (trangThaiMoi == TrangThaiDon.HoanThanh)
            {
                if (giaoNhan == null ||
                    giaoNhan.HinhThuc != don.HinhThucNhan ||
                    giaoNhan.ThoiGianDuKien < don.NgayDat.AddMinutes(30))
                {
                    return (false, "Chưa có phiếu giao nhận hợp lệ.");
                }

                if (don.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi &&
                    (string.IsNullOrWhiteSpace(giaoNhan.DiaChi) ||
                     string.IsNullOrWhiteSpace(don.NguoiNhan) ||
                     string.IsNullOrWhiteSpace(don.SoDienThoaiNguoiNhan)))
                {
                    return (false, "Thiếu thông tin người nhận hoặc địa chỉ giao.");
                }

                if (giaoNhan.TrangThai == TrangThaiGiaoNhanConst.HoanThanh)
                {
                    return (false, "Giao nhận đã được hoàn thành.");
                }

                var daThanhToan = await _db.ThanhToans
                    .Where(t => t.MaDon == maDon)
                    .SumAsync(t => (decimal?)t.SoTien) ?? 0m;

                if (daThanhToan < don.TongTien)
                {
                    return (false, "Đơn chưa thanh toán đủ.");
                }

                giaoNhan.TrangThai = TrangThaiGiaoNhanConst.HoanThanh;
                giaoNhan.ThoiGianThucTe = DateTime.Now;
            }

            // Cập nhật trạng thái nếu dữ liệu chưa bị thay đổi.
            var soDong = await _db.DonDatHoas
                .Where(d => d.MaDon == maDon &&
                            d.TrangThai == don.TrangThai)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(d => d.TrangThai, trangThaiMoi));

            if (soDong != 1)
            {
                return (false, "Đơn vừa thay đổi, vui lòng tải lại.");
            }

            // Ghi lại lịch sử xử lý đơn.
            var lichSu = new LichSuXuLyDon
            {
                MaDon = maDon,
                TrangThaiCu = don.TrangThai,
                TrangThaiMoi = trangThaiMoi,
                ThoiGian = DateTime.Now,
                NguoiThucHien = nguoiThucHien,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu)
                    ? "Chuyển trạng thái theo quy trình"
                    : ghiChu.Trim()
            };

            _db.LichSuXuLyDons.Add(lichSu);

            // Lưu dữ liệu.
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Chuyển trạng thái thành công và đã lưu lịch sử.");
        }

    }
}
