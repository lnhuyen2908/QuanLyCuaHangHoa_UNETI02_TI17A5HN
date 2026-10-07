
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    public class GioHangController : Controller
    {
        // GET: /User/Cart
        public IActionResult Index()
        {
            return View();
        }
    }
}
