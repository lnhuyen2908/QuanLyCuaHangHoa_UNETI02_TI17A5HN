
using Microsoft.AspNetCore.Mvc;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Products")]
    public class MauSanPhamController : Controller
    {
        // GET: /User/Products or /User/MauSanPham
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index(string search, string category, string priceRange, string sort)
        {
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.PriceRange = priceRange;
            ViewBag.Sort = sort;
            return View("~/Areas/User/Views/MauSanPham/Index.cshtml");
        }

        // GET: /User/Products/Detail/1 or /User/MauSanPham/Detail/1
        [HttpGet("Detail/{id?}")]
        public IActionResult Detail(int? id)
        {
            ViewBag.ProductId = id ?? 1;
            return View("~/Areas/User/Views/MauSanPham/Detail.cshtml");
        }
    }
}
