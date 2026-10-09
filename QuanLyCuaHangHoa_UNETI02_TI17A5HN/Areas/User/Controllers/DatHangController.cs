
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Checkout")]
    public class DatHangController : Controller
    {
        // GET: /User/Checkout or /User/DatHang
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/User/Views/DatHang/Index.cshtml");
        }
    }
}
