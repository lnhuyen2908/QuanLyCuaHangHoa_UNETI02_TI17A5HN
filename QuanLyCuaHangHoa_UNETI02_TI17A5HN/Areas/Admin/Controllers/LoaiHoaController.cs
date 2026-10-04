using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoaiHoaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
