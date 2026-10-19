// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Hiển thị danh sách, tìm kiếm, lọc, sắp xếp và phân trang mẫu hoa; xem chi tiết mẫu hoa

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [Route("User/[controller]")]
    [Route("User/Products")]
    public class MauSanPhamController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 12;

        public MauSanPhamController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maDip,
            string? kieuSanPham,
            decimal? giaTu,
            decimal? giaDen,
            string? sapXep,
            string? mauSac,
            int page = 1)
        {
            if (giaTu < 0) giaTu = null;
            if (giaDen < 0) giaDen = null;
            if (giaTu.HasValue && giaDen.HasValue && giaTu > giaDen)
            {
                (giaTu, giaDen) = (giaDen, giaTu);
            }

            var query = _context.MauSanPhams
                .AsNoTracking()
                .Include(m => m.DipSuDung)
                .Where(m => m.TrangThai &&
                            m.DipSuDung != null &&
                            m.DipSuDung.TrangThai);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(m =>
                    m.TenMau.Contains(tuKhoa) ||
                    m.KieuSanPham.Contains(tuKhoa) ||
                    (m.DipSuDung != null && m.DipSuDung.TenDip.Contains(tuKhoa)));
            }

            if (maDip.HasValue)
                query = query.Where(m => m.MaDip == maDip.Value);

            if (!string.IsNullOrWhiteSpace(kieuSanPham))
            {
                kieuSanPham = kieuSanPham.Trim();
                query = query.Where(m => m.KieuSanPham == kieuSanPham);
            }

            if (!string.IsNullOrWhiteSpace(mauSac))
            {
                mauSac = mauSac.Trim();
                query = query.Where(m =>
                    m.ChiTietMaus.Any(ct =>
                        ct.HoaTuoi != null && ct.HoaTuoi.MauSac == mauSac));
            }

            if (giaTu.HasValue)
                query = query.Where(m => m.GiaCoBan >= giaTu.Value);

            if (giaDen.HasValue)
                query = query.Where(m => m.GiaCoBan <= giaDen.Value);

            switch (sapXep)
            {
                case "gia-tang":
                    query = query.OrderBy(m => m.GiaCoBan).ThenBy(m => m.MaMau);
                    break;
                case "gia-giam":
                    query = query.OrderByDescending(m => m.GiaCoBan).ThenByDescending(m => m.MaMau);
                    break;
                case "ten-a-z":
                    query = query.OrderBy(m => m.TenMau).ThenBy(m => m.MaMau);
                    break;
                case "ten-z-a":
                    query = query.OrderByDescending(m => m.TenMau).ThenByDescending(m => m.MaMau);
                    break;
                default:
                    sapXep = "moi-nhat";
                    query = query.OrderByDescending(m => m.MaMau);
                    break;
            }

            var danhSachMau = await PagedList<MauSanPham>.CreateAsync(query, page, PageSize);

            var danhSachDip = await _context.DipSuDungs
                .AsNoTracking()
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDip)
                .Select(d => new SelectListItem
                {
                    Text = d.TenDip,
                    Value = d.MaDip.ToString()
                })
                .ToListAsync();

            var danhSachKieu = new List<SelectListItem>
            {
                new() { Text = "Bó hoa", Value = "Bó hoa" },
                new() { Text = "Giỏ hoa", Value = "Giỏ hoa" },
                new() { Text = "Hộp hoa", Value = "Hộp hoa" },
                new() { Text = "Kệ hoa", Value = "Kệ hoa" },
                new() { Text = "Mẫu khác", Value = "Mẫu khác" }
            };

            var model = new MauSanPhamListViewModel
            {
                DanhSachMau = danhSachMau,
                TuKhoa = tuKhoa,
                MaDip = maDip,
                KieuSanPham = kieuSanPham,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SapXep = sapXep,
                MauSac = mauSac,
                TrangHienTai = danhSachMau.TrangHienTai,
                SoBanGhiMoiTrang = danhSachMau.SoBanGhiMoiTrang,
                DanhSachDip = danhSachDip,
                DanhSachKieu = danhSachKieu
            };

            return View("~/Areas/User/Views/MauSanPham/Index.cshtml", model);
        }

        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var model = await _context.MauSanPhams
                .AsNoTracking()
                .Include(m => m.DipSuDung)
                .Include(m => m.ChiTietMaus)
                    .ThenInclude(c => c.HoaTuoi)
                .FirstOrDefaultAsync(m =>
                    m.MaMau == id &&
                    m.TrangThai &&
                    m.DipSuDung != null &&
                    m.DipSuDung.TrangThai);

            if (model == null) return NotFound();

            var mauLienQuan = await _context.MauSanPhams
                .AsNoTracking()
                .Where(m => m.TrangThai
                    && m.MaMau != id
                    && m.DipSuDung != null
                    && m.DipSuDung.TrangThai)
                .OrderByDescending(m => m.MaMau)
                .Take(4)
                .ToListAsync();

            ViewBag.MauLienQuan = mauLienQuan;

            return View("~/Areas/User/Views/MauSanPham/Detail.cshtml", model);
        }
    }
}
