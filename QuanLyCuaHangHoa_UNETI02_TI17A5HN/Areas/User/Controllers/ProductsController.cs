
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    public class ProductsController : Controller
    {
        // GET: /User/Products
        public IActionResult Index(string search, string category, string priceRange, string sort)
        {
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.PriceRange = priceRange;
            ViewBag.Sort = sort;
            return View();
        }

        // GET: /User/Products/Detail/1
        public IActionResult Detail(int? id)
        {
            ViewBag.ProductId = id ?? 1;
            return View();
        }
    }
}
