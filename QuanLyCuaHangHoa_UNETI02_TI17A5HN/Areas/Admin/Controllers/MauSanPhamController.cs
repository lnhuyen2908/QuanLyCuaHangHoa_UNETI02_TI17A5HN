// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Controller Mẫu sản phẩm - áp dụng quy tắc Dịp sử dụng: ngừng hoạt động thì không dùng cho mẫu mới.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class MauSanPhamController : Controller
    {
        private readonly AppDbContext _context;

        public MauSanPhamController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách dịp đang hoạt động (loại bỏ các dịp đã ngừng hoạt động) để dùng cho mẫu mới
            ViewBag.DanhSachDip = await _context.DipSuDungs
                .AsNoTracking()
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDip)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Create()
        {
            // Quy tắc: Ngừng hoạt động thì không dùng cho mẫu mới -> chỉ lấy TrangThai == true
            ViewBag.DanhSachDip = await _context.DipSuDungs
                .AsNoTracking()
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDip)
                .ToListAsync();

            return View();
        }
    }
}
