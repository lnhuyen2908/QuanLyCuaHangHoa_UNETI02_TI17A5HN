
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Backwards compatibility redirects for legacy routes
        public IActionResult Products() => RedirectToAction("Index", "MauSanPham");
        public IActionResult Detail() => RedirectToAction("Detail", "MauSanPham");
        public IActionResult ProductDetail() => RedirectToAction("Detail", "MauSanPham");
        public IActionResult Cart() => RedirectToAction("Index", "GioHang");
        public IActionResult Checkout() => RedirectToAction("Index", "DatHang");
        public IActionResult CustomDesign() => RedirectToAction("Index", "ThietKeRieng");
        public IActionResult Profile() => RedirectToAction("Index", "HoSo");
        public IActionResult Wishlist() => RedirectToAction("Index", "YeuThich");
        public IActionResult Register() => RedirectToAction("Register", "TaiKhoan");
        public IActionResult Login() => RedirectToAction("Login", "TaiKhoan");
    }
}
