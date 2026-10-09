
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/CustomDesign")]
    [Route("User/CustomOrder")]
    public class ThietKeRiengController : Controller
    {
        // GET: /User/CustomDesign or /User/ThietKeRieng
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/User/Views/ThietKeRieng/Index.cshtml");
        }
    }
}
