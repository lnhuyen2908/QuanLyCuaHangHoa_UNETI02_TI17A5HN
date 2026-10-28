
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
    public class HomeController : Controller
    {
        // BỔ SUNG MODULE 4 - Nguyễn Thị Đào
        // MSSV: 23103100245
        // Dashboard thống kê dữ liệu thật bằng EF Core LINQ.

        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;
            var ngayMai = homNay.AddDays(1);
            var dauThang = new DateTime(
                homNay.Year, homNay.Month, 1);
            var dauThangSau = dauThang.AddMonths(1);
            var dau7Ngay = homNay.AddDays(-6);

            var vm = new DashboardViewModel();

            // 1. Thống kê số đơn theo trạng thái
            var trangThais = await _db.DonDatHoas
                .AsNoTracking()
                .GroupBy(d => d.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.TrangThai,
                    x => x.SoLuong);

            int Dem(string trangThai) =>
                trangThais.GetValueOrDefault(trangThai);

            vm.TongDon = trangThais.Values.Sum();
            vm.ChoXacNhan = Dem(TrangThaiDon.ChoXacNhan);
            vm.DaXacNhan = Dem(TrangThaiDon.DaXacNhan);
            vm.DangChuanBi = Dem(TrangThaiDon.DangChuanBi);
            vm.SanSang = Dem(TrangThaiDon.SanSang);
            vm.DangGiao = Dem(TrangThaiDon.DangGiao);
            vm.HoanThanh = Dem(TrangThaiDon.HoanThanh);
            vm.DaHuy = Dem(TrangThaiDon.DaHuy);

            // 2. Doanh thu ngày: tiền thực tế đã thu
            vm.DoanhThuNgay = await _db.ThanhToans
                .AsNoTracking()
                .Where(t =>
                    t.NgayThanhToan >= homNay &&
                    t.NgayThanhToan < ngayMai)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0m;

            // 3. Doanh thu tháng
            vm.DoanhThuThang = await _db.ThanhToans
                .AsNoTracking()
                .Where(t =>
                    t.NgayThanhToan >= dauThang &&
                    t.NgayThanhToan < dauThangSau)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0m;

            // 4. Phiếu giao tận nơi dự kiến hôm nay
            vm.GiaoHomNay = await _db.GiaoNhans
                .AsNoTracking()
                .CountAsync(g =>
                    g.HinhThuc == HinhThucNhanConst.GiaoTanNoi &&
                    g.ThoiGianDuKien >= homNay &&
                    g.ThoiGianDuKien < ngayMai);

            // 5. Đơn chưa thanh toán đủ (không tính đơn hủy)
            vm.DonConThieuTien = await _db.DonDatHoas
                .AsNoTracking()
                .CountAsync(d =>
                    d.TrangThai != TrangThaiDon.DaHuy &&
                    d.TongTien >
                    (d.ThanhToans.Sum(t =>
                        (decimal?)t.SoTien) ?? 0m));

            // 6. Top 5 mẫu hoa được đặt nhiều nhất
            // Không tính số lượng trong đơn đã hủy.
            vm.MauBanChay = await (
                from c in _db.ChiTietDonHoas.AsNoTracking()
                join m in _db.MauSanPhams.AsNoTracking()
                    on c.MaMau equals (int?)m.MaMau
                where c.DonDatHoa.TrangThai
                    != TrangThaiDon.DaHuy
                group c by new { m.MaMau, m.TenMau } into g
                orderby g.Sum(c => c.SoLuong) descending
                select new DashboardMauBanChay
                {
                    MaMau = g.Key.MaMau,
                    TenMau = g.Key.TenMau,
                    SoLuong = g.Sum(c => c.SoLuong)
                }
            ).Take(5).ToListAsync();

            // 7. Doanh thu 7 ngày gần nhất
            var doanhThu = await _db.ThanhToans
                .AsNoTracking()
                .Where(t =>
                    t.NgayThanhToan >= dau7Ngay &&
                    t.NgayThanhToan < ngayMai)
                .GroupBy(t => t.NgayThanhToan.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    SoTien = g.Sum(t => t.SoTien)
                })
                .ToListAsync();

            vm.DoanhThu7Ngay = Enumerable
                .Range(0, 7)
                .Select(i =>
                {
                    var ngay = dau7Ngay.AddDays(i);
                    return new DashboardDiemDoanhThu
                    {
                        Ngay = ngay,
                        SoTien = doanhThu
                            .FirstOrDefault(x => x.Ngay == ngay)
                            ?.SoTien ?? 0m
                    };
                })
                .ToList();

            // 8. Các đơn cần xử lý trong hôm nay
            vm.DonCanXuLy = await _db.DonDatHoas
                .AsNoTracking()
                .Where(d =>
                // TẠM THỜI TEST CÁC ĐƠN HẸN GIAO NGÀY MAI
                    d.ThoiGianNhanGiao >= homNay &&
                    d.ThoiGianNhanGiao < ngayMai &&
                    d.TrangThai != TrangThaiDon.HoanThanh &&
                    d.TrangThai != TrangThaiDon.DaHuy)
                .OrderBy(d => d.ThoiGianNhanGiao)
                .Select(d => new DashboardDonCanXuLy
                {
                    MaDon = d.MaDon,
                    NguoiNhan = d.NguoiNhan,
                    ThoiGianNhanGiao = d.ThoiGianNhanGiao,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai,
                    // Hiển thị mẫu đầu tiên của đơn;
                    // đơn thiết kế riêng không có MaMau.
                    TenMau = d.ChiTietDonHoas
                        .OrderBy(ct => ct.MaChiTiet)
                        .Select(ct => ct.MaMau != null
                            ? ct.MauSanPham!.TenMau
                            : "Thiết kế riêng")
                        .FirstOrDefault() ?? "Chưa có chi tiết"
                })
                .Take(6)
                .ToListAsync();

            // Nguyễn Thị Đào - 23103100245
            // Thống kê sức tiêu thụ theo kiểu sản phẩm bằng LINQ và EF Core.
            // Tổng số sản phẩm đã đặt theo từng kiểu.
            // Không tính đơn đã hủy và mẫu thiết kế riêng
            // không có MaMau.
            var soLuongTheoKieu = await (
                from ct in _db.ChiTietDonHoas.AsNoTracking()
                join mau in _db.MauSanPhams.AsNoTracking()
                    on ct.MaMau equals (int?)mau.MaMau
                where ct.DonDatHoa.TrangThai
                    != TrangThaiDon.DaHuy
                group ct by mau.KieuSanPham into nhom
                select new
                {
                    TenKieu = nhom.Key,
                    SoLuong = nhom.Sum(x => x.SoLuong)
                }
            ).ToListAsync();

            // Tổng số lượng mẫu có sẵn đã được đặt.
            int tongSoLuong = soLuongTheoKieu
                .Sum(x => x.SoLuong);

            // Bốn kiểu sản phẩm của hệ thống.
            var danhSachKieu = new[]
            {
                KieuSanPhamConst.BoHoa,
                KieuSanPhamConst.GioHoa,
                KieuSanPhamConst.HopHoa,
                KieuSanPhamConst.KeHoa
            };

            // Tính tỷ lệ phần trăm từng kiểu.
            vm.ThongKeKieuSanPham = danhSachKieu
                .Select(kieu =>
                {
                    int soLuong = soLuongTheoKieu
                        .FirstOrDefault(x => x.TenKieu == kieu)
                        ?.SoLuong ?? 0;

                    return new DashboardThongKeKieu
                    {
                        TenKieu = kieu,
                        SoLuong = soLuong,
                        TyLe = tongSoLuong > 0
                            ? Math.Round(
                                soLuong * 100.0 / tongSoLuong, 1)
                            : 0
                    };
                })
                .ToList();

            // KẾT THÚC THỐNG KÊ KIỂU SẢN PHẨM

            // Nguyễn Thị Đào - 23103100245
            // Dashboard: yêu cầu thiết kế riêng chờ báo giá.
            var yeuCauChoGia = _db.ChiTietDonHoas
                .AsNoTracking()
                .Where(ct =>
                    ct.MaMau == null &&
                    !ct.DaChotGia &&
                    ct.DonDatHoa.TrangThai
                        == TrangThaiDon.ChoXacNhan);

            // Tổng số yêu cầu đang chờ chốt giá.
            vm.SoYeuCauChoGia =
                await yeuCauChoGia.CountAsync();

            // Yêu cầu mới nhất cần xử lý.
            vm.YeuCauMoiNhat = await yeuCauChoGia
                .OrderByDescending(ct => ct.DonDatHoa.NgayDat)
                .ThenByDescending(ct => ct.MaChiTiet)
                .Select(ct => new DashboardYeuCauThietKe
                {
                    MaChiTiet = ct.MaChiTiet,
                    MaDon = ct.MaDon,
                    TenKhach = ct.DonDatHoa.KhachHang.HoTen,
                    NoiDung = ct.YeuCauRieng ?? "",
                    NgayDat = ct.DonDatHoa.NgayDat
                })
                .FirstOrDefaultAsync();
            return View(vm);
        }

    }
}
