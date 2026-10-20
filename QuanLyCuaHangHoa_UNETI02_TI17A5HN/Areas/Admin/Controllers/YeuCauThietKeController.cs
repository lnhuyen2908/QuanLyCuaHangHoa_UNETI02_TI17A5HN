// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Danh sách yêu cầu thiết kế riêng và chốt giá từng chi tiết theo POST, cập nhật tổng tiền.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class YeuCauThietKeController : Controller
    {
        private readonly AppDbContext _db;

        public YeuCauThietKeController(AppDbContext db) => _db = db;

        // GET: danh sách những chi tiết không thuộc mẫu có sẵn (MaMau == null).
        public async Task<IActionResult> Index(string? trangThai)
        {
            ViewBag.TrangThai = trangThai;

            var q = _db.ChiTietDonHoas
                .AsNoTracking()
                .Include(x => x.DonDatHoa)
                    .ThenInclude(x => x.KhachHang)
                .Where(x => x.MaMau == null);

            // Lọc "cho" (chưa có báo giá) hoặc "da" (đã báo giá); giới hạn tối đa 200 kết quả.
            if (trangThai == "cho")
            {
                q = q.Where(x => !x.DaChotGia);
            }
            else if (trangThai == "da")
            {
                q = q.Where(x => x.DaChotGia);
            }

            return View(await q
                .OrderByDescending(x => x.DonDatHoa.NgayDat)
                .Take(200)
                .ToListAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        // POST chốt giá một chi tiết; không chốt lặp và không chốt đơn đã đi tiếp quy trình.
        public async Task<IActionResult> ChotGia(int maChiTiet, decimal donGia)
        {
            // Từ chối giá âm/0, vượt ngưỡng hoặc có quá 2 chữ số thập phân.
            if (donGia <= 0 || donGia > 999999999m || decimal.Round(donGia, 2) != donGia)
            {
                TempData["Error"] = "Giá chốt phải lớn hơn 0, tối đa 999.999.999 và có tối đa 2 chữ số thập phân.";
                return RedirectToAction(nameof(Index));
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Tải đơn cha để kiểm tra trạng thái và các khoản phí hiện có.
            var item = await _db.ChiTietDonHoas
                .Include(x => x.DonDatHoa)
                .FirstOrDefaultAsync(x => x.MaChiTiet == maChiTiet && x.MaMau == null);

            if (item == null)
            {
                return NotFound();
            }

            if (item.DaChotGia
                || item.DonDatHoa.TrangThai != TrangThaiDon.ChoXacNhan
                || await _db.ThanhToans.AnyAsync(x => x.MaDon == item.MaDon))
            {
                TempData["Error"] = "Chỉ chốt giá yêu cầu chưa báo giá, đơn chờ xác nhận và chưa thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Tính tiền có kiểm tra tràn số; cập nhật chi tiết theo điều kiện chưa chốt giá.
            var thanhTien = checked(donGia * item.SoLuong);

            var updated = await _db.ChiTietDonHoas
                .Where(x => x.MaChiTiet == maChiTiet && !x.DaChotGia)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.DonGia, donGia)
                    .SetProperty(x => x.ThanhTien, thanhTien)
                    .SetProperty(x => x.DaChotGia, true));

            if (updated != 1)
            {
                TempData["Error"] = "Yêu cầu đã được chốt giá bởi người khác.";
                return RedirectToAction(nameof(Index));
            }

            // SQL update để đồng bộ tổng tiền với tất cả chi tiết, không dùng dữ liệu cũ do EF cache.
            var sum = await _db.ChiTietDonHoas
                .AsNoTracking()
                .Where(x => x.MaDon == item.MaDon)
                .SumAsync(x => x.ThanhTien);

            // Tổng đơn = tiền các dòng chi tiết + phí giao hàng + phụ phí; Module 4 dùng tổng này.
            var newTotal = sum + item.DonDatHoa.PhiGiaoHang + item.DonDatHoa.PhuPhi;

            await _db.DonDatHoas
                .Where(x => x.MaDon == item.MaDon && x.TrangThai == TrangThaiDon.ChoXacNhan)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.TongTien, newTotal));

            // Chốt giá KHÔNG đổi trạng thái: lưu ghi chú để nhân viên vẫn xác nhận ở bước kế tiếp.
            _db.LichSuXuLyDons.Add(new LichSuXuLyDon
            {
                MaDon = item.MaDon,
                TrangThaiCu = TrangThaiDon.ChoXacNhan,
                TrangThaiMoi = TrangThaiDon.ChoXacNhan,
                ThoiGian = DateTime.Now,
                NguoiThucHien = HttpContext.Session.GetMaTaiKhoan()!.Value,
                GhiChu = $"Chốt giá thiết kế riêng #{maChiTiet}: {donGia:N0}đ"
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "Đã chốt giá; đơn vẫn chờ xác nhận để bộ phận đơn hàng tiếp tục xử lý.";
            return RedirectToAction(nameof(Index));
        }
    }
}