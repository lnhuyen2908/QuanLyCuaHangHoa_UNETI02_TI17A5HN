
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - Xem lịch sử và ghi nhận
// thanh toán nhiều lần, không vượt tổng tiền đơn.

using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class ThanhToanController : Controller
    {
        // kết nối csdl
        private readonly AppDbContext _db;

        public ThanhToanController(AppDbContext db)
        {
            _db = db;
        }

        // Nguyễn Thị Đào - Chọn đơn, tra cứu và xem lịch sử thanh toán.
        // Danh sách có tìm kiếm, lọc tình trạng và phân trang để xử lý nhiều đơn.
        public async Task<IActionResult> Index(
            int? maDon,
            string? tuKhoa,
            string? tinhTrangThanhToan,
            int page = 1)
        {
            var query = _db.DonDatHoas
                .AsNoTracking()
                .Where(d => d.TrangThai != TrangThaiDon.DaHuy)
                .Select(d => new ThanhToanDonHangViewModel
                {
                    MaDon = d.MaDon,
                    NguoiNhan = d.NguoiNhan,
                    SoDienThoaiNguoiNhan = d.SoDienThoaiNguoiNhan,
                    NgayDat = d.NgayDat,
                    TrangThaiDon = d.TrangThai,
                    TongTien = d.TongTien,
                    TongDaThanhToan = _db.ThanhToans
                        .Where(t => t.MaDon == d.MaDon)
                        .Sum(t => (decimal?)t.SoTien) ?? 0m
                });

            // Tìm theo mã đơn, tên hoặc số điện thoại người nhận.
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                var maTimKiem = tuKhoa;

                if (maTimKiem.StartsWith("#ORD-", StringComparison.OrdinalIgnoreCase))
                    maTimKiem = maTimKiem.Substring(5);
                else if (maTimKiem.StartsWith("ORD-", StringComparison.OrdinalIgnoreCase))
                    maTimKiem = maTimKiem.Substring(4);
                else
                    maTimKiem = maTimKiem.TrimStart('#');

                if (int.TryParse(maTimKiem, out int ma))
                {
                    query = query.Where(d =>
                        d.MaDon == ma ||
                        d.NguoiNhan.Contains(tuKhoa) ||
                        d.SoDienThoaiNguoiNhan.Contains(tuKhoa));
                }
                else
                {
                    query = query.Where(d =>
                        d.NguoiNhan.Contains(tuKhoa) ||
                        d.SoDienThoaiNguoiNhan.Contains(tuKhoa));
                }
            }

            // Lọc theo số tiền còn phải thu hoặc đã thanh toán đủ.
            if (tinhTrangThanhToan == "con-no")
            {
                query = query.Where(d => d.TongDaThanhToan < d.TongTien);
            }
            else if (tinhTrangThanhToan == "da-du")
            {
                query = query.Where(d => d.TongDaThanhToan >= d.TongTien);
            }
            else
            {
                tinhTrangThanhToan = "";
            }

            query = query.OrderByDescending(d => d.MaDon);

            var vm = new ThanhToanViewModel
            {
                MaDon = maDon,
                TuKhoa = tuKhoa,
                TinhTrangThanhToan = tinhTrangThanhToan,
                DanhSachDonHang = await PagedList<ThanhToanDonHangViewModel>
                    .CreateAsync(query, page, 10)
            };

            // Nếu đã chọn một đơn thì nạp tổng tiền và lịch sử thanh toán của đơn đó.
            if (maDon.HasValue)
            {
                var don = await _db.DonDatHoas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d =>
                        d.MaDon == maDon.Value &&
                        d.TrangThai != TrangThaiDon.DaHuy);

                if (don == null)
                    return NotFound();

                vm.TongTien = don.TongTien;

                vm.LichSu = await _db.ThanhToans
                    .AsNoTracking()
                    .Where(t => t.MaDon == maDon.Value)
                    .OrderByDescending(t => t.NgayThanhToan)
                    .ToListAsync();

                vm.TongDaThanhToan = vm.LichSu.Sum(t => t.SoTien);

                var ids = vm.LichSu
                    .Select(t => t.NguoiGhiNhan)
                    .Distinct()
                    .ToList();

                vm.TenNhanVien = await _db.TaiKhoans
                    .AsNoTracking()
                    .Where(t => ids.Contains(t.MaTaiKhoan))
                    .ToDictionaryAsync(
                        t => t.MaTaiKhoan,
                        t => t.HoTen);
            }

            return View(vm);
        }

        // Nguyễn Thị Đào - Mở form ghi nhận thanh toán.
        [HttpGet]
        public async Task<IActionResult> Create(int maDon)
        {
            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (don == null)
                return NotFound();

            if (don.TrangThai == TrangThaiDon.DaHuy)
                return BadRequest("Đơn đã hủy không nhận thanh toán.");

            var daTra = await _db.ThanhToans
                .Where(t => t.MaDon == maDon)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0m;

            if (daTra >= don.TongTien)
                return RedirectToAction(nameof(Index), new { maDon });

            // hiển thị trang Create.html cho nv nhập số tiền và pttt
            return View(new ThanhToanCreateViewModel
            {
                MaDon = maDon,
                TongTien = don.TongTien,
                TongDaThanhToan = daTra,
                PhuongThuc = PhuongThucThanhToanConst.TienMat
            });
        }

        // Nguyễn Thị Đào - Ghi nhận thanh toán vào SQL Server.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ThanhToanCreateViewModel vm)
        {
            // Hiện tại hệ thống chỉ triển khai ghi nhận thanh toán tiền mặt.
            // Các phương thức khác chỉ hiển thị trên giao diện, không được phép gửi lên server.
            if (vm.PhuongThuc != PhuongThucThanhToanConst.TienMat)
            {
                ModelState.AddModelError(
                    nameof(vm.PhuongThuc),
                    "Hiện tại chỉ hỗ trợ ghi nhận thanh toán bằng tiền mặt.");
            }

            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
                return Unauthorized();

            // Khóa giao dịch để tránh hai nhân viên cùng ghi thanh toán vượt tổng tiền.
            await using var tx =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDon == vm.MaDon);

            if (don == null)
                return NotFound();

            var daTra = await _db.ThanhToans
                .Where(t => t.MaDon == vm.MaDon)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0m;

            vm.TongTien = don.TongTien;
            vm.TongDaThanhToan = daTra;

            if (don.TrangThai == TrangThaiDon.DaHuy)
            {
                ModelState.AddModelError(
                    "", "Không thanh toán đơn đã hủy.");
            }

            if (vm.SoTien > vm.SoTienConLai)
            {
                ModelState.AddModelError(
                    nameof(vm.SoTien),
                    "Số tiền vượt số tiền còn lại.");
            }

            if (vm.SoTienConLai <= 0)
            {
                ModelState.AddModelError(
                    "", "Đơn đã thanh toán đủ.");
            }
            //thanh toán k hợp lệ
            if (!ModelState.IsValid)
                return View(vm);

            _db.ThanhToans.Add(new ThanhToan
            {
                MaDon = vm.MaDon,
                SoTien = vm.SoTien,
                PhuongThuc = vm.PhuongThuc,
                GhiChu = vm.GhiChu?.Trim(),
                NguoiGhiNhan = maTaiKhoan.Value,
                NgayThanhToan = DateTime.Now
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync(); // ghi nhận giao dịch

            TempData["Success"] =
                "Đã ghi nhận thanh toán thành công.";

            return RedirectToAction(
                nameof(Index), new { maDon = vm.MaDon });
        }

        // Nguyễn Thị Đào - Xem lịch sử chi tiết giao dịch thanh toán
        [HttpGet]
        public async Task<IActionResult> LichSu(int maDon)
        {
            // 1. Lấy thông tin đơn hàng
            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (don == null)
                return NotFound();

            // 2. Lấy toàn bộ giao dịch của đơn
            var giaoDich = await _db.ThanhToans
                .AsNoTracking()
                .Where(t => t.MaDon == maDon)
                .OrderByDescending(t => t.NgayThanhToan)
                .Select(t => new
                {
                    t.MaThanhToan,
                    t.NgayThanhToan,
                    t.SoTien,
                    t.PhuongThuc,
                    t.NguoiGhiNhan,
                    t.GhiChu
                })
                .ToListAsync();

            // 3. Lấy tên nhân viên ghi nhận
            var ids = giaoDich
                .Select(t => t.NguoiGhiNhan)
                .Distinct()
                .ToList();

            var tenNhanVien = await _db.TaiKhoans
                .AsNoTracking()
                .Where(t => ids.Contains(t.MaTaiKhoan))
                .ToDictionaryAsync(
                    t => t.MaTaiKhoan,
                    t => t.HoTen
                );

            // 4. Chuyển dữ liệu sang ViewModel
            var danhSach = new List<ThanhToanLichSuItemViewModel>();

            foreach (var gd in giaoDich)
            {
                tenNhanVien.TryGetValue(
                    gd.NguoiGhiNhan, out var ten);

                danhSach.Add(new ThanhToanLichSuItemViewModel
                {
                    MaThanhToan = gd.MaThanhToan,
                    NgayThanhToan = gd.NgayThanhToan,
                    SoTien = gd.SoTien,
                    PhuongThuc = gd.PhuongThuc,
                    TenNguoiGhiNhan = ten ?? "Không xác định",
                    GhiChu = gd.GhiChu
                });
            }

            // 5. Tính tổng tiền
            var tongDaThanhToan = danhSach.Sum(t => t.SoTien);

            var vm = new ThanhToanLichSuViewModel
            {
                DonHang = don,
                GiaoDich = danhSach,
                TongDaThanhToan = tongDaThanhToan,
                SoTienConLai = Math.Max(
                    0, don.TongTien - tongDaThanhToan)
            };

            return View(vm);
        }
    }
}
