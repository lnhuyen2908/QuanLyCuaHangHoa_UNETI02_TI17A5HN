
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

        // Nguyễn Thị Đào - Xem thông tin và lịch sử thanh toán.
        public async Task<IActionResult> Index(int? maDon)
        {
            var vm = new ThanhToanViewModel
            {
                MaDon = maDon                   // gán sẵn mã đơn hàng thu tiền
            };

            vm.DonHang = await _db.DonDatHoas
                .AsNoTracking()
                .Where(d => d.TrangThai != TrangThaiDon.DaHuy)
                .OrderByDescending(d => d.MaDon)
                .ToListAsync();

            if (maDon.HasValue)
            {
                var don = await _db.DonDatHoas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaDon == maDon.Value);

                if (don == null)
                    return NotFound();

                vm.TongTien = don.TongTien;

                vm.LichSu = await _db.ThanhToans
                    .AsNoTracking()
                    .Where(t => t.MaDon == maDon.Value)
                    .OrderByDescending(t => t.NgayThanhToan)
                    .ToListAsync();

                vm.TongDaThanhToan =
                    vm.LichSu.Sum(t => t.SoTien);

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
                TongDaThanhToan = daTra
            });
        }

        // Nguyễn Thị Đào - Ghi nhận thanh toán vào SQL Server.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ThanhToanCreateViewModel vm)
        {
            var phuongThucs = new[]
            {
                PhuongThucThanhToanConst.TienMat,
                PhuongThucThanhToanConst.ChuyenKhoan,
                PhuongThucThanhToanConst.ViDienTu,
                PhuongThucThanhToanConst.TheNganHang
            };

            if (!phuongThucs.Contains(vm.PhuongThuc))
            {
                ModelState.AddModelError(
                    nameof(vm.PhuongThuc),
                    "Phương thức thanh toán không hợp lệ.");
            }

            var maTaiKhoan =  HttpContext.Session.GetMaTaiKhoan();

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
    }
}
