
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    public class WishlistController : Controller
    {
        // GET: /User/Wishlist
        public IActionResult Index()
        {
            return View();
        }
    }
}
