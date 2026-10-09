
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Cart")]
    public class GioHangController : Controller
    {
        // GET: /User/Cart or /User/GioHang
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/User/Views/GioHang/Index.cshtml");
        }
    }
}
