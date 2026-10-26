
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Thống kê LINQ nâng cao - Module 4.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _db;

        public ThongKeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new ThongKeViewModel();

            // 1. DOANH THU THỰC THU THEO THÁNG
            var thangHienTai = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month, 1);

            var thangBatDau = thangHienTai.AddMonths(-11);

            var duLieuThang = await _db.ThanhToans
                .AsNoTracking()
                .Where(t => t.NgayThanhToan >= thangBatDau
                    && t.NgayThanhToan <
                        thangHienTai.AddMonths(1))
                .GroupBy(t => new
                {
                    t.NgayThanhToan.Year,
                    t.NgayThanhToan.Month
                })
                .Select(g => new ThongKeDoanhThuThang
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoTien = g.Sum(t => t.SoTien)
                })
                .ToListAsync();

            vm.DoanhThuTheoThang = Enumerable.Range(0, 12)
                .Select(i =>
                {
                    var thang = thangBatDau.AddMonths(i);

                    return new ThongKeDoanhThuThang
                    {
                        Nam = thang.Year,
                        Thang = thang.Month,
                        SoTien = duLieuThang
                            .Where(x => x.Nam == thang.Year
                                && x.Thang == thang.Month)
                            .Sum(x => x.SoTien)
                    };
                })
                .ToList();

            // 2. SỐ ĐƠN THEO TRẠNG THÁI
            vm.DonTheoTrangThai = await _db.DonDatHoas
                .AsNoTracking()
                .GroupBy(d => d.TrangThai)
                .Select(g => new ThongKeSoLuong
                {
                    Ten = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            // 3. GIÁ TRỊ SẢN PHẨM THEO KIỂU
            // Chỉ xét đơn hoàn thành, không tính phí.
            vm.GiaTriTheoKieu = await (
                from ct in _db.ChiTietDonHoas.AsNoTracking()
                join mau in _db.MauSanPhams.AsNoTracking()
                    on ct.MaMau equals (int?)mau.MaMau
                where ct.DonDatHoa.TrangThai
                    == TrangThaiDon.HoanThanh
                group ct by mau.KieuSanPham into g
                select new ThongKeSoTien
                {
                    Ten = g.Key,
                    SoTien = g.Sum(x => x.ThanhTien)
                }
            )
            .OrderByDescending(x => x.SoTien)
            .ToListAsync();

            // 4. GIÁ TRỊ SẢN PHẨM THEO DỊP SỬ DỤNG
            vm.GiaTriTheoDip = await (
                from ct in _db.ChiTietDonHoas.AsNoTracking()
                join mau in _db.MauSanPhams.AsNoTracking()
                    on ct.MaMau equals (int?)mau.MaMau
                join dip in _db.DipSuDungs.AsNoTracking()
                    on mau.MaDip equals dip.MaDip
                where ct.DonDatHoa.TrangThai
                    == TrangThaiDon.HoanThanh
                group ct by dip.TenDip into g
                select new ThongKeSoTien
                {
                    Ten = g.Key,
                    SoTien = g.Sum(x => x.ThanhTien)
                }
            )
            .OrderByDescending(x => x.SoTien)
            .ToListAsync();

            // 5. TOP 5 MẪU SẢN PHẨM BÁN NHIỀU
            vm.MauBanChay = await (
                from ct in _db.ChiTietDonHoas.AsNoTracking()
                join mau in _db.MauSanPhams.AsNoTracking()
                    on ct.MaMau equals (int?)mau.MaMau
                where ct.DonDatHoa.TrangThai
                    == TrangThaiDon.HoanThanh
                group ct by mau.TenMau into g
                select new ThongKeSoLuong
                {
                    Ten = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong)
                }
            )
            .OrderByDescending(x => x.SoLuong)
            .Take(5)
            .ToListAsync();

            // 6. TOP 5 LOẠI HOA PHỔ BIẾN
            // Số lượt xuất hiện trong mẫu đã bán.
            vm.LoaiHoaPhoBien = await (
                from ct in _db.ChiTietDonHoas.AsNoTracking()
                join chiTietMau in _db.ChiTietMaus.AsNoTracking()
                    on ct.MaMau equals (int?)chiTietMau.MaMau
                join hoa in _db.HoaTuois.AsNoTracking()
                    on chiTietMau.MaHoa equals hoa.MaHoa
                join loai in _db.LoaiHoas.AsNoTracking()
                    on hoa.MaLoaiHoa equals loai.MaLoaiHoa
                where ct.DonDatHoa.TrangThai
                    == TrangThaiDon.HoanThanh
                group ct by loai.TenLoaiHoa into g
                select new ThongKeSoLuong
                {
                    Ten = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong)
                }
            )
            .OrderByDescending(x => x.SoLuong)
            .Take(5)
            .ToListAsync();


            // 7. TOP 5 KHÁCH HÀNG CÓ NHIỀU ĐƠN NHẤT
            // Không tính đơn đã hủy.
            vm.KhachHangNhieuDon = await _db.DonDatHoas
                .AsNoTracking()
                .Where(d => d.TrangThai
                    != TrangThaiDon.DaHuy)
                .GroupBy(d => new
                {
                    d.MaKhachHang,
                    d.KhachHang.HoTen
                })
                .Select(g => new ThongKeSoLuong
                {
                    Ten = g.Key.HoTen,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToListAsync();

            // 8. GIÁ TRỊ ĐƠN TRUNG BÌNH
            // Không tính đơn hủy hoặc chưa có giá.
            vm.GiaTriDonTrungBinh =
                await _db.DonDatHoas
                    .AsNoTracking()
                    .Where(d =>
                        d.TrangThai != TrangThaiDon.DaHuy &&
                        d.TongTien > 0)
                    .AverageAsync(d => (decimal?)d.TongTien)
                ?? 0m;

            // 9. ĐIỂM ĐÁNH GIÁ TRUNG BÌNH
            vm.TongDanhGia = await _db.DanhGias
                .AsNoTracking()
                .CountAsync();

            vm.DiemDanhGiaTrungBinh =
                await _db.DanhGias
                    .AsNoTracking()
                    .AverageAsync(d => (double?)d.SoSao)
                ?? 0;

            return View(vm);
        }
    }
}
