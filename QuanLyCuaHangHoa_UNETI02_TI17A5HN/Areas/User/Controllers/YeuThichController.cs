
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Wishlist")]
    public class YeuThichController : Controller
    {
        // GET: /User/Wishlist or /User/YeuThich
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/User/Views/YeuThich/Index.cshtml");
        }
    }
}
