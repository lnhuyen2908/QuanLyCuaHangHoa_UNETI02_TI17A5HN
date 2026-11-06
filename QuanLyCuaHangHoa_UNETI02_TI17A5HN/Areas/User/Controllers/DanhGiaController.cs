
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - Khách hàng đánh giá đơn hoàn thành.
// Kiểm tra quyền sở hữu, trạng thái và đánh giá trùng.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [PhanQuyen(VaiTroConst.KhachHang)]
    public class DanhGiaController : Controller
    {
        private readonly AppDbContext _db;

        public DanhGiaController(AppDbContext db)
        {
            _db = db;
        }

        // Nguyễn Thị Đào - Mở form đánh giá.
        [HttpGet]
        public async Task<IActionResult> Create(int maDon)
        {
            var maTaiKhoan =
                HttpContext.Session.GetMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
                return Unauthorized();

            // Chỉ tìm đơn của tài khoản đăng nhập.
            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.MaDon == maDon &&
                    d.KhachHang.MaTaiKhoan == maTaiKhoan.Value);

            if (don == null)
                return NotFound();

            if (don.TrangThai != TrangThaiDon.HoanThanh)
            {
                TempData["Loi"] =
                    "Chỉ được đánh giá đơn đã hoàn thành.";

                return Redirect("/User/Profile#orders");
            }

            // Chỉ cho phép đánh giá trong vòng 10 ngày kể từ thời điểm đơn hoàn thành.
            var thoiGianHoanThanh = await _db.LichSuXuLyDons
                .AsNoTracking()
                .Where(ls => ls.MaDon == maDon &&
                             ls.TrangThaiMoi == TrangThaiDon.HoanThanh)
                .OrderByDescending(ls => ls.ThoiGian)
                .Select(ls => (DateTime?)ls.ThoiGian)
                .FirstOrDefaultAsync();

            if (thoiGianHoanThanh.HasValue &&
                DateTime.Now > thoiGianHoanThanh.Value.AddDays(10))
            {
                TempData["Loi"] =
                    "Đã quá 10 ngày kể từ khi đơn hoàn thành. Bạn không thể đánh giá đơn hàng này.";
                return Redirect("/User/Profile#orders");
            }

            bool daDanhGia = await _db.DanhGias
                .AnyAsync(d => d.MaDon == maDon);

            if (daDanhGia)
            {
                TempData["ThongBao"] =
                    "Đơn này đã được đánh giá.";

                return Redirect("/User/Profile#orders");
            }

            return View(new DanhGiaCreateViewModel
            {
                MaDon = maDon
            });
        }

        // Nguyễn Thị Đào - Lưu đánh giá vào SQL Server.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DanhGiaCreateViewModel vm)
        {
            var maTaiKhoan =
                HttpContext.Session.GetMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
                return Unauthorized();

            // thông tin mã khách hàng từ trình duyệt.
            // Kiểm tra lại chủ sở hữu ngay tại POST.
            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.MaDon == vm.MaDon &&
                    d.KhachHang.MaTaiKhoan == maTaiKhoan.Value);

            if (don == null)
                return NotFound();

            if (don.TrangThai != TrangThaiDon.HoanThanh)
            {
                ModelState.AddModelError("",
                    "Chỉ được đánh giá đơn đã hoàn thành.");
            }

            // Kiểm tra lại ở POST để không thể bỏ qua giới hạn bằng cách gửi form cũ.
            var thoiGianHoanThanh = await _db.LichSuXuLyDons
                .AsNoTracking()
                .Where(ls => ls.MaDon == vm.MaDon &&
                             ls.TrangThaiMoi == TrangThaiDon.HoanThanh)
                .OrderByDescending(ls => ls.ThoiGian)
                .Select(ls => (DateTime?)ls.ThoiGian)
                .FirstOrDefaultAsync();

            if (thoiGianHoanThanh.HasValue &&
                DateTime.Now > thoiGianHoanThanh.Value.AddDays(10))
            {
                ModelState.AddModelError("",
                    "Đã quá 10 ngày kể từ khi đơn hoàn thành. Bạn không thể đánh giá đơn hàng này.");
            }

            if (await _db.DanhGias
                .AnyAsync(d => d.MaDon == vm.MaDon))
            {
                ModelState.AddModelError("",
                    "Đơn hàng đã có đánh giá.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var danhGia = new DanhGia
            {
                MaDon = don.MaDon,
                MaKhachHang = don.MaKhachHang,
                SoSao = vm.SoSao,
                NoiDung = string.IsNullOrWhiteSpace(vm.NoiDung)
                    ? null
                    : vm.NoiDung.Trim(),
                NgayDanhGia = DateTime.Now
            };

            _db.DanhGias.Add(danhGia);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)  // giả sử SaveChange thấy bại vì đã có đánh giá
            {
                // CSDL đã có ràng buộc 1 đơn - 1 đánh giá.
                // Xử lý khi hai yêu cầu đánh giá đến cùng lúc.
                _db.Entry(danhGia).State = EntityState.Detached; // -> sẽ loại đối tượng đó khỏi danh sách EF Core đang theo dõi.

                if (await _db.DanhGias
                    .AnyAsync(d => d.MaDon == vm.MaDon))
                {
                    ModelState.AddModelError("",
                        "Đơn vừa được đánh giá, không thể gửi trùng.");
                    return View(vm);
                }

                throw;
            }

            TempData["ThongBao"] =
                "Cảm ơn bạn! Đánh giá đã được lưu thành công.";

            return Redirect("/User/Profile#orders");
        }
    }
}
