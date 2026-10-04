
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        // GET: /User/Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: /User/Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
