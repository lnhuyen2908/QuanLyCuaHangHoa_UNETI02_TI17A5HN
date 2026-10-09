
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Profile")]
    [Route("User/Customer")]
    public class HoSoController : Controller
    {
        // GET: /User/Profile or /User/HoSo
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/User/Views/HoSo/Index.cshtml");
        }
    }
}
