
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Account")]
    public class TaiKhoanController : Controller
    {
        // GET: /User/Account/Login or /User/TaiKhoan/Login
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View("~/Areas/User/Views/TaiKhoan/Login.cshtml");
        }

        // GET: /User/Account/Register or /User/TaiKhoan/Register
        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View("~/Areas/User/Views/TaiKhoan/Register.cshtml");
        }

        // POST: /User/Account/Logout or /User/TaiKhoan/Logout
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
