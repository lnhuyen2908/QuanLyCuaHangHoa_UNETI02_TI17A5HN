
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
        public IActionResult Products() => RedirectToAction("Index", "Products");
        public IActionResult Detail() => RedirectToAction("Detail", "Products");
        public IActionResult ProductDetail() => RedirectToAction("Detail", "Products");
        public IActionResult Cart() => RedirectToAction("Index", "Cart");
        public IActionResult Checkout() => RedirectToAction("Index", "Checkout");
        public IActionResult CustomDesign() => RedirectToAction("Index", "CustomDesign");
        public IActionResult Profile() => RedirectToAction("Index", "Profile");
        public IActionResult Wishlist() => RedirectToAction("Index", "Wishlist");
        public IActionResult Register() => RedirectToAction("Register", "Account");
        public IActionResult Login() => RedirectToAction("Login", "Account");
    }
}
