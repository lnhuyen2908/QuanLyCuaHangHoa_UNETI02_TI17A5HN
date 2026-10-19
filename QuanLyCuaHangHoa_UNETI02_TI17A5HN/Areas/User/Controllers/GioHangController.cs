// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Quản lý giỏ Session có kiểm tra DB, cập nhật số lượng, xóa và hiển thị tổng tiền.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using System.Text.Json;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Cart")]
    [PhanQuyen]
    public class GioHangController : Controller
    {
        private readonly AppDbContext _db;
        private readonly GioHangService _cart;
        public GioHangController(AppDbContext db, GioHangService cart)
        {
            _db = db;
            _cart = cart;
        }

        // MaTaiKhoan được lấy từ phiên đăng nhập, không nhận từ form phía client.
        private int Account
            => HttpContext.Session.GetMaTaiKhoan() ?? throw new InvalidOperationException("Cần đăng nhập.");

        // GET: đọc giỏ của tài khoản hiện tại và hiển thị dữ liệu sản phẩm lấy từ DB.
        [HttpGet(""), HttpGet("Index")]
        public async Task<IActionResult> Index()
            => View("~/Areas/User/Views/GioHang/Index.cshtml", await _cart.DisplayAsync(HttpContext.Session, Account));

        // POST /User/Cart/Add: chỉ thêm mẫu còn bán, giới hạn số lượng tối đa 100/mẫu.
        [HttpPost("Add"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int maMau, int soLuong = 1)
        {
            if (maMau <= 0 || soLuong is < 1 or > 100)
            {
                TempData["Error"] = "Mã mẫu hoặc số lượng không hợp lệ (tối đa 100).";
                return RedirectToAction(nameof(Index));
            }

            if (!await _db.MauSanPhams.AnyAsync(x => x.MaMau == maMau && x.TrangThai))
            {
                TempData["Error"] = "Mẫu hoa đã ngừng bán hoặc không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Có sẵn trong giỏ thì cộng số lượng; chưa có thì tạo dòng CartItem mới.
            var items = _cart.Read(HttpContext.Session, Account);
            var item = items.FirstOrDefault(x => x.MaMau == maMau);

            if (item == null)
            {
                items.Add(new CartItem { MaMau = maMau, SoLuong = soLuong });
            }
            else if (item.SoLuong + soLuong <= 100)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                TempData["Error"] = "Tối đa 100 sản phẩm mỗi mẫu.";
                return RedirectToAction(nameof(Index));
            }

            // Session chỉ được ghi lại sau khi toàn bộ điều kiện thêm đều hợp lệ.
            _cart.Save(HttpContext.Session, Account, items);
            TempData["Success"] = "Đã thêm vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // POST /User/Cart/Update: thay thế số lượng (không cộng dồn) cho mẫu đang bán.
        [HttpPost("Update"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int maMau, int soLuong)
        {
            var items = _cart.Read(HttpContext.Session, Account);
            var item = items.FirstOrDefault(x => x.MaMau == maMau);

            if (item == null
                || soLuong is < 1 or > 100
                || !await _db.MauSanPhams.AnyAsync(x => x.MaMau == maMau && x.TrangThai))
            {
                TempData["Error"] = "Không thể cập nhật mẫu hoa hoặc số lượng.";
                return RedirectToAction(nameof(Index));
            }

            item.SoLuong = soLuong;
            _cart.Save(HttpContext.Session, Account, items);
            TempData["Success"] = "Đã cập nhật số lượng.";
            return RedirectToAction(nameof(Index));
        }

        // POST /User/Cart/Remove: bỏ một mẫu ra khỏi giỏ; không đụng tới DB đơn hàng.
        [HttpPost("Remove"), ValidateAntiForgeryToken]
        public IActionResult Remove(int maMau)
        {
            var items = _cart.Read(HttpContext.Session, Account);
            items.RemoveAll(x => x.MaMau == maMau);
            _cart.Save(HttpContext.Session, Account, items);
            return RedirectToAction(nameof(Index));
        }

        // POST /User/Cart/Clear: xóa toàn bộ giỏ Session của tài khoản hiện tại.
        [HttpPost("Clear"), ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear(HttpContext.Session, Account);
            return RedirectToAction(nameof(Index));
        }
    }
}