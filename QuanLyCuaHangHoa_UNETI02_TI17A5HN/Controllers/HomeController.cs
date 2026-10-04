// Họ và tên: Lã Ngọc Huyền (Nhóm QuanLyCuaHangHoa_UNETI02_TI17A5HN)
// Nội dung thực hiện: Controller mặc định điều hướng về Giao diện Khách hàng thuộc Area User (Areas/User/Views/Home/)

using Microsoft.AspNetCore.Mvc;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using System.Diagnostics;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        public IActionResult Products()
        {
            return RedirectToAction("Products", "Home", new { area = "User" });
        }

        public IActionResult Detail()
        {
            return RedirectToAction("Detail", "Home", new { area = "User" });
        }

        public IActionResult ProductDetail()
        {
            return RedirectToAction("Detail", "Home", new { area = "User" });
        }

        public IActionResult Cart()
        {
            return RedirectToAction("Cart", "Home", new { area = "User" });
        }

        public IActionResult Checkout()
        {
            return RedirectToAction("Checkout", "Home", new { area = "User" });
        }

        public IActionResult CustomDesign()
        {
            return RedirectToAction("CustomDesign", "Home", new { area = "User" });
        }

        public IActionResult About()
        {
            return RedirectToAction("About", "Home", new { area = "User" });
        }

        public IActionResult Contact()
        {
            return RedirectToAction("Contact", "Home", new { area = "User" });
        }

        public IActionResult Profile()
        {
            return RedirectToAction("Profile", "Home", new { area = "User" });
        }

        public IActionResult Wishlist()
        {
            return RedirectToAction("Wishlist", "Home", new { area = "User" });
        }

        public IActionResult Register()
        {
            return RedirectToAction("Register", "Home", new { area = "User" });
        }

        public IActionResult Login()
        {
            return RedirectToAction("Login", "Home", new { area = "User" });
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Privacy", "Home", new { area = "User" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
