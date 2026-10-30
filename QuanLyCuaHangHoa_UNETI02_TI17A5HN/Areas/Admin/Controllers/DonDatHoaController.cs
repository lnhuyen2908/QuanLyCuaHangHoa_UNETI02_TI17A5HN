// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Quản lý đơn Admin: lọc/phân trang, chi tiết, xác nhận đúng điều kiện và hủy có lịch sử

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
// BỔ SUNG MODULE 4 - Nguyễn Thị Đào (23103100245)
// Sử dụng Service xử lý trạng thái đơn hàng.
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class DonDatHoaController : Controller
    {
        private readonly AppDbContext _db;

        public DonDatHoaController(AppDbContext db) => _db = db;

        // GET danh sách: thống kê chung độc lập với bộ lọc bảng; truy vấn không tracking để giảm tải.
        public async Task<IActionResult> Index(DonDatHoaListViewModel vm)
        {
            // Mặc định lọc đơn Chờ xác nhận khi mới vào trang.
            // Nếu Admin tự chọn trạng thái khác hoặc Tất cả thì giữ lựa chọn đó.
            if (!Request.Query.ContainsKey("trangThai"))
            {
                vm.TrangThai = TrangThaiDon.ChoXacNhan;
            }

            var today = DateTime.Today;
            var baseQ = _db.DonDatHoas.AsNoTracking();

            vm.DonHomNay = await baseQ.CountAsync(x => x.NgayDat >= today && x.NgayDat < today.AddDays(1));

            vm.DoanhSoHomNay = await baseQ
                .Where(x => x.NgayDat >= today
                    && x.NgayDat < today.AddDays(1)
                    && x.TrangThai == TrangThaiDon.HoanThanh)
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;

            vm.ChoXacNhan = await baseQ.CountAsync(x => x.TrangThai == TrangThaiDon.ChoXacNhan);
            vm.DangChuanBi = await baseQ.CountAsync(x => x.TrangThai == TrangThaiDon.DangChuanBi);
            vm.SanSang = await baseQ.CountAsync(x => x.TrangThai == TrangThaiDon.SanSang);
            vm.DangGiao = await baseQ.CountAsync(x => x.TrangThai == TrangThaiDon.DangGiao);

            vm.HoanThanhThang = await baseQ.CountAsync(x =>
                x.TrangThai == TrangThaiDon.HoanThanh
                && x.NgayDat >= new DateTime(today.Year, today.Month, 1));

            // Bắt đầu xây dựng IQueryable, chỉ thực thi SQL sau khi thêm điều kiện lọc.
            var q = baseQ.AsQueryable();

            // Tìm theo mã đơn dạng số/#ORD-, tên người nhận, số điện thoại hoặc tên khách.
            if (!string.IsNullOrWhiteSpace(vm.TuKhoa))
            {
                var keyword = vm.TuKhoa.Trim();

                if (int.TryParse(keyword.Replace("#ORD-", "", StringComparison.OrdinalIgnoreCase), out var id))
                {
                    q = q.Where(x => x.MaDon == id || x.NguoiNhan.Contains(keyword));
                }
                else
                {
                    q = q.Where(x =>
                        x.NguoiNhan.Contains(keyword)
                        || x.SoDienThoaiNguoiNhan.Contains(keyword)
                        || x.KhachHang.HoTen.Contains(keyword));
                }
            }

            // Bộ lọc trạng thái và khoảng ngày cùng áp dụng trên danh sách kết quả.
            if (!string.IsNullOrWhiteSpace(vm.TrangThai))
            {
                q = q.Where(x => x.TrangThai == vm.TrangThai);
            }

            if (vm.TuNgay.HasValue)
            {
                q = q.Where(x => x.NgayDat >= vm.TuNgay.Value.Date);
            }

            if (vm.DenNgay.HasValue)
            {
                q = q.Where(x => x.NgayDat < vm.DenNgay.Value.Date.AddDays(1));
            }

            // Phân trang 10 đơn/trang; Clamp tránh truy cập trang âm/vượt tổng số trang.
            vm.TongBanGhi = await q.CountAsync();
            vm.TongTrang = Math.Max(1, (vm.TongBanGhi + 9) / 10);
            vm.Trang = Math.Clamp(vm.Trang, 1, vm.TongTrang);
            vm.DanhSach = await q
                .Include(x => x.KhachHang)
                .OrderByDescending(x => x.NgayDat)
                .ThenByDescending(x => x.MaDon)
                .Skip((vm.Trang - 1) * 10)
                .Take(10)
                .ToListAsync();

            return View(vm);
        }

        // GET chi tiết: lấy kèm sản phẩm, thanh toán, giao nhận, lịch sử để Module 4 phối hợp.
        public async Task<IActionResult> Details(int id)
        {
            var d = await _db.DonDatHoas
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.KhachHang)
                .Include(x => x.ChiTietDonHoas)
                    .ThenInclude(x => x.MauSanPham)
                .Include(x => x.LichSuXuLyDons)
                .Include(x => x.GiaoNhans)
                .Include(x => x.ThanhToans)
                .FirstOrDefaultAsync(x => x.MaDon == id);

            // Lấy họ tên những tài khoản đã thực hiện xử lý đơn.
            if (d != null)
            {
                var maTaiKhoans = d.LichSuXuLyDons
                    .Select(x => x.NguoiThucHien)
                    .Distinct()
                    .ToList();

                var nguoiThucHiens = await _db.TaiKhoans
                    .AsNoTracking()
                    .Where(x => maTaiKhoans.Contains(x.MaTaiKhoan))
                    .Select(x => new
                    {
                        x.MaTaiKhoan,
                        x.HoTen
                    })
                    .ToListAsync();

                ViewBag.TenNguoiThucHien = nguoiThucHiens
                    .ToDictionary(x => x.MaTaiKhoan, x => x.HoTen);
            }

            return d == null
                ? NotFound()
                : View(new DonDatHoaDetailViewModel { Don = d });
        }

        [HttpPost, ValidateAntiForgeryToken]
        // POST xác nhận: chỉ đơn Chờ xác nhận và không còn chi tiết thiết kế chưa chốt giá.
        public async Task<IActionResult> XacNhan(int id)
        {
            // Atomic compare-and-set: ngăn 2 nhân viên cùng xác nhận, mỗi lần đổi trạng thái đúng 1 lịch sử.
            await using var tx = await _db.Database.BeginTransactionAsync();

            var d = await _db.DonDatHoas.AsNoTracking().FirstOrDefaultAsync(x => x.MaDon == id);
            if (d == null)
            {
                return NotFound();
            }

            // Lấy toàn bộ chi tiết của đơn cần xác nhận.
            var chiTiets = await _db.ChiTietDonHoas
                .AsNoTracking()
                .Where(x => x.MaDon == id)
                .ToListAsync();

            // 1. Đúng trạng thái.
            bool dungTrangThai =
                d.TrangThai == TrangThaiDon.ChoXacNhan;

            // 2. Có sản phẩm và tất cả chi tiết hợp lệ.
            bool dungChiTiet =
                chiTiets.Count > 0 &&
                chiTiets.All(x =>
                    x.SoLuong > 0 &&
                    x.DonGia > 0 &&
                    x.DaChotGia &&
                    x.ThanhTien == x.SoLuong * x.DonGia);

            // 3. Thông tin giao nhận hợp lệ.
            bool dungGiaoNhan =
                !string.IsNullOrWhiteSpace(d.NguoiNhan) &&
                !string.IsNullOrWhiteSpace(d.SoDienThoaiNguoiNhan) &&
                (
                    d.HinhThucNhan == HinhThucNhanConst.KhachDenNhan ||
                    (
                        d.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi &&
                        !string.IsNullOrWhiteSpace(d.DiaChiGiao)
                    )
                );

            // 4. Thời gian giao/nhận phải sau lúc đặt.
            bool dungThoiGian =
                d.ThoiGianNhanGiao > d.NgayDat;

            // 5. Tổng tiền phải khớp dữ liệu đã lưu.
            bool dungTongTien =
                d.PhiGiaoHang >= 0 &&
                d.PhuPhi >= 0 &&
                d.TongTien ==
                    chiTiets.Sum(x => x.ThanhTien)
                    + d.PhiGiaoHang
                    + d.PhuPhi;

            if (!dungTrangThai || !dungChiTiet ||
                !dungGiaoNhan || !dungThoiGian || !dungTongTien)
            {
                TempData["Error"] =
                    "Đơn chưa đủ điều kiện xác nhận. " +
                    "Kiểm tra chi tiết, báo giá, giao nhận và tổng tiền.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // ExecuteUpdate có điều kiện trạng thái cũ: chỉ một lần POST được thay đổi đơn.
            var changed = await _db.DonDatHoas
                .Where(x => x.MaDon == id && x.TrangThai == TrangThaiDon.ChoXacNhan)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, TrangThaiDon.DaXacNhan));

            if (changed != 1)
            {
                TempData["Error"] = "Đơn đã được người khác xử lý, vui lòng tải lại.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Lịch sử lưu người xác nhận và trạng thái trước/sau để truy vết.
            _db.LichSuXuLyDons.Add(new LichSuXuLyDon
            {
                MaDon = id,
                TrangThaiCu = TrangThaiDon.ChoXacNhan,
                TrangThaiMoi = TrangThaiDon.DaXacNhan,
                ThoiGian = DateTime.Now,
                NguoiThucHien = HttpContext.Session.GetMaTaiKhoan()!.Value,
                GhiChu = "Nhân viên xác nhận đơn"
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "Đã xác nhận đơn hàng.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        // POST hủy Admin: yêu cầu lý do; từ chối đơn đang giao, đã kết thúc hoặc đã thanh toán.
        public async Task<IActionResult> Huy(int id, string? lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo) || lyDo.Trim().Length > 500)
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy (tối đa 500 ký tự).";
                return RedirectToAction(nameof(Details), new { id });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            var d = await _db.DonDatHoas.AsNoTracking().FirstOrDefaultAsync(x => x.MaDon == id);
            if (d == null)
            {
                return NotFound();
            }

            if (d.TrangThai is TrangThaiDon.DaHuy or TrangThaiDon.HoanThanh or TrangThaiDon.DangGiao
                || await _db.ThanhToans.AnyAsync(x => x.MaDon == id))
            {
                TempData["Error"] = "Đơn đang giao, đã kết thúc hoặc đã có thanh toán cần xử lý theo quy trình riêng.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Bản ghi chỉ đổi khi trạng thái chưa bị nhân viên khác cập nhật.
            var changed = await _db.DonDatHoas
                .Where(x => x.MaDon == id && x.TrangThai == d.TrangThai)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.TrangThai, TrangThaiDon.DaHuy)
                    .SetProperty(x => x.LyDoHuy, lyDo.Trim()));

            if (changed != 1)
            {
                TempData["Error"] = "Đơn vừa thay đổi, vui lòng tải lại.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Ghi lịch sử bằng trạng thái thực tế trước khi hủy; thao tác nằm trong transaction.
            _db.LichSuXuLyDons.Add(new LichSuXuLyDon
            {
                MaDon = id,
                TrangThaiCu = d.TrangThai,
                TrangThaiMoi = TrangThaiDon.DaHuy,
                ThoiGian = DateTime.Now,
                NguoiThucHien = HttpContext.Session.GetMaTaiKhoan()!.Value,
                GhiChu = "Hủy đơn: " + lyDo.Trim()
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction(nameof(Details), new { id });
        }


        // Họ và tên: Nguyễn Thị Đào
        // Mã sinh viên: 23103100245
        // Nội dung thực hiện: Tuần 6 - Module 4
        // Chuyển trạng thái đơn và ghi lịch sử xử lý.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuyenTrangThai(
            int id, string? trangThaiMoi, string? ghiChu)
        {
            if (id <= 0)
                return NotFound();

            if (string.IsNullOrWhiteSpace(trangThaiMoi))
            {
                TempData["Error"] = "Bạn chưa chọn trạng thái mới.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
                return Unauthorized();

            var service = new TrangThaiDonService(_db);

            var ketQua = await service.ChuyenAsync(
                id,
                trangThaiMoi,
                maTaiKhoan.Value,
                ghiChu);

            TempData[ketQua.ThanhCong ? "Success" : "Error"]
                = ketQua.ThongBao;

            return RedirectToAction(nameof(Details), new { id });
        }
        // KẾT THÚC PHẦN MODULE 4 - Nguyễn Thị Đào

    }
}