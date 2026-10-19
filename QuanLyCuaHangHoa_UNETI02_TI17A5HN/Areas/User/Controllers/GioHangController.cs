// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Xây dựng chức năng quản lý giỏ hàng bằng Session.
// Bao gồm: hiển thị, thêm, sửa số lượng và xóa mẫu hoa khỏi giỏ hàng.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using System.Text.Json;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Cart")]
    [PhanQuyen]
    public class GioHangController : Controller
    {
        // DbContext và constructor
        private readonly AppDbContext _context;

        public GioHangController(AppDbContext context)
        {
            _context = context;
        }

        // Session Key
        private string CartSessionKey
        {
            get
            {
                var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();

                if (!maTaiKhoan.HasValue)
                    throw new InvalidOperationException(
                        "Bạn cần đăng nhập để sử dụng giỏ hàng.");
                return $"GioHang_{maTaiKhoan.Value}";
            }
        }

        // Đọc danh sách sản phẩm từ Session.
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(json)
                       ?? new List<CartItem>();
            }
            catch (JsonException)
            {
                return new List<CartItem>();
            }
        }

        // Lưu danh sách sản phẩm vào Session.
        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        // GET: /User/Cart hoặc /User/GioHang
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var cart = GetCart();
            return View("~/Areas/User/Views/GioHang/Index.cshtml", cart);
        }

        // POST: /User/Cart/Add
        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int maMau, int soLuong = 1)
        {
            // Kiểm tra dữ liệu đầu vào.
            if (maMau <= 0 || soLuong <= 0)
            {
                TempData["Error"] = "Mã mẫu hoa hoặc số lượng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Bắt buộc bổ sung truy vấn CSDL để xác nhận:
            // 1. Mẫu hoa có tồn tại.
            // 2. Mẫu hoa đang hoạt động.
            // Không được cho phép thêm sản phẩm khi chưa kiểm tra.

            if (!await KiemTraMauHoa(maMau))
            {
                TempData["Error"] = "Mẫu hoa không tồn tại hoặc đã ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();

            // Kiểm tra mẫu hoa đã có trong giỏ chưa.
            var item = cart.FirstOrDefault(x => x.MaMau == maMau);

            if (item == null)
            {
                // Nếu chưa có thì thêm mới.
                cart.Add(new CartItem
                {
                    MaMau = maMau,
                    SoLuong = soLuong
                });
            }
            else
            {
                // Tránh tràn số nguyên khi cộng số lượng.
                if (item.SoLuong > int.MaxValue - soLuong)
                {
                    TempData["Error"] = "Số lượng sản phẩm vượt quá giới hạn.";
                    return RedirectToAction(nameof(Index));
                }

                // Nếu đã có thì cộng thêm số lượng.
                item.SoLuong += soLuong;
            }

            SaveCart(cart);

            TempData["Success"] = "Đã thêm mẫu hoa vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /User/Cart/Update
        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int maMau, int soLuong)
        {
            // Không chấp nhận số lượng bằng 0 hoặc âm.
            if (maMau <= 0 || soLuong <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction(nameof(Index));
            }

            if (!await KiemTraMauHoa(maMau))
            {
                TempData["Error"] = "Mẫu hoa không tồn tại hoặc đã ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaMau == maMau);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy mẫu hoa trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật số lượng mới.
            item.SoLuong = soLuong;
            SaveCart(cart);
            TempData["Success"] = "Đã cập nhật số lượng sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /User/Cart/Remove
        [HttpPost("Remove")]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int maMau)
        {
            var cart = GetCart();
            // Xóa mẫu hoa theo mã mẫu.
            int soLuongDaXoa =
                cart.RemoveAll(x => x.MaMau == maMau);

            if (soLuongDaXoa == 0)
            {
                TempData["Error"] = "Không tìm thấy mẫu hoa trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            SaveCart(cart);
            TempData["Success"] = "Đã xóa mẫu hoa khỏi giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /User/Cart/Clear
        // Phục vụ nút "Xóa toàn bộ giỏ hàng" đang có trên View.
        [HttpPost("Clear")]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra mẫu hoa từ database
        private async Task<bool> KiemTraMauHoa(int maMau)
        {
            return await _context.MauSanPhams
                .AnyAsync(m => m.MaMau == maMau && m.TrangThai);
        }
    }
}