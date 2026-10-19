// Họ và tên: Nguyễn Thị Phương Anh
// MSSV: 23103100267
// Module 2: Mẫu sản phẩm, chi tiết thành phần, tìm kiếm và phân trang.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers;

[Area("Admin")]
[PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
public class MauSanPhamController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private const int PageSize = 10;
    private static readonly string[] KieuMau = { "Bó hoa", "Giỏ hoa", "Hộp hoa", "Kệ hoa", "Mẫu khác" };

    public MauSanPhamController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? maDip, string? kieuSanPham,
        decimal? giaTu, decimal? giaDen, string? trangThai, string? sapXep, int page = 1)
    {
        // Thống kê luôn lấy toàn bộ dữ liệu, không phụ thuộc bộ lọc tìm kiếm.
        var tongMau = await _context.MauSanPhams.AsNoTracking().CountAsync();
        var dangMoBan = await _context.MauSanPhams.AsNoTracking().CountAsync(m => m.TrangThai);
        var tamNgung = await _context.MauSanPhams.AsNoTracking().CountAsync(m => !m.TrangThai);
        var soDipSuDung = await _context.MauSanPhams.AsNoTracking()
            .Select(m => m.MaDip).Distinct().CountAsync();

        ViewBag.TongMau = tongMau;
        ViewBag.DangMoBan = dangMoBan;
        ViewBag.TamNgung = tamNgung;
        ViewBag.SoDipSuDung = soDipSuDung;

        var query = _context.MauSanPhams.AsNoTracking()
            .Include(m => m.DipSuDung)
            .Include(m => m.ChiTietMaus).ThenInclude(c => c.HoaTuoi)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var key = tuKhoa.Trim();
            query = query.Where(m => m.TenMau.Contains(key) ||
                (m.MoTa != null && m.MoTa.Contains(key)) ||
                m.DipSuDung.TenDip.Contains(key) ||
                m.KieuSanPham.Contains(key));
        }
        if (maDip.HasValue) query = query.Where(m => m.MaDip == maDip.Value);
        if (!string.IsNullOrWhiteSpace(kieuSanPham)) query = query.Where(m => m.KieuSanPham == kieuSanPham);
        if (giaTu.HasValue) query = query.Where(m => m.GiaCoBan >= giaTu.Value);
        if (giaDen.HasValue) query = query.Where(m => m.GiaCoBan <= giaDen.Value);
        if (trangThai == "true") query = query.Where(m => m.TrangThai);
        else if (trangThai == "false") query = query.Where(m => !m.TrangThai);

        query = sapXep switch
        {
            "ten_asc" => query.OrderBy(m => m.TenMau),
            "ten_desc" => query.OrderByDescending(m => m.TenMau),
            "gia_asc" => query.OrderBy(m => m.GiaCoBan),
            "gia_desc" => query.OrderByDescending(m => m.GiaCoBan),
            "cu_nhat" => query.OrderBy(m => m.MaMau),
            _ => query.OrderBy(m => m.MaMau)
        };

        var count = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
        var model = new MauSanPhamListViewModel
        {
            DanhSachMau = new PagedList<MauSanPham>
            {
                DuLieu = items,
                TrangHienTai = page,
                TongSoBanGhi = count,
                SoBanGhiMoiTrang = PageSize
            },
            TuKhoa = tuKhoa,
            MaDip = maDip,
            KieuSanPham = kieuSanPham,
            GiaTu = giaTu,
            GiaDen = giaDen,
            TrangThai = trangThai,
            SapXep = sapXep,
            TrangHienTai = page,
            SoBanGhiMoiTrang = PageSize,
            DanhSachDip = await _context.DipSuDungs.AsNoTracking().OrderBy(d => d.TenDip)
                .Select(d => new SelectListItem(d.TenDip, d.MaDip.ToString())).ToListAsync(),
            DanhSachKieu = KieuMau.Select(k => new SelectListItem(k, k)).ToList()
        };

        ViewData["AdminFilter"] = new AdminFilterViewModel
        {
            Area = "Admin",
            Controller = "MauSanPham",
            Action = "Index",

            TuKhoa = tuKhoa,
            TrangThai = trangThai == "true"
                ? true
                : trangThai == "false" ? false : null,

            SapXep = sapXep,
            Placeholder = "Tìm tên mẫu, dịp sử dụng, kiểu sản phẩm...",
            HienThiTrangThai = true,

            SapXepOptions = new List<AdminSortOption>
            {
                new() { Value = "", Text = "Mã mẫu tăng dần" },
                new() { Value = "ten_asc", Text = "Tên A-Z" },
                new() { Value = "ten_desc", Text = "Tên Z-A" },
                new() { Value = "gia_asc", Text = "Giá tăng dần" },
                new() { Value = "gia_desc", Text = "Giá giảm dần" },
                new() { Value = "cu_nhat", Text = "Cũ nhất" }
            },

            HienThiBoLocMauSanPham = true,
            MaDip = maDip,
            KieuSanPham = kieuSanPham,
            GiaTu = giaTu,
            GiaDen = giaDen,

            DanhSachDip = await _context.DipSuDungs
        .AsNoTracking()
        .OrderBy(d => d.TenDip)
        .Select(d => new SelectListItem(
            d.TenDip, d.MaDip.ToString()))
        .ToListAsync(),

            DanhSachKieuSanPham = KieuMau
        .Select(k => new SelectListItem(k, k))
        .ToList()
        };

        ViewData["Pagination"] = new PaginationViewModel
        {
            Area = "Admin",
            Controller = "MauSanPham",
            Action = "Index",
            TrangHienTai = page,
            TongSoBanGhi = count,
            SoBanGhiMoiTrang = PageSize,
            BoLoc = new Dictionary<string, string?>
            {
                ["tuKhoa"] = tuKhoa,
                ["maDip"] = maDip?.ToString(),
                ["kieuSanPham"] = kieuSanPham,
                ["giaTu"] = giaTu?.ToString(),
                ["giaDen"] = giaDen?.ToString(),
                ["trangThai"] = trangThai,
                ["sapXep"] = sapXep
            }
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new MauSanPhamFormViewModel();
        await NapDanhSachAsync(model, chiLayDipHoatDong: true);
        return View(model);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MauSanPhamFormViewModel model)
    {
        // Kiểm tra tên mẫu đã tồn tại chưa
        string tenMau = model.TenMau?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenMau))
        {
            ModelState.AddModelError(
                nameof(model.TenMau),
                "Vui lòng nhập tên mẫu sản phẩm."
            );
        }
        else
        {
            bool tenDaTonTai = await _context.MauSanPhams
                .AnyAsync(x => x.TenMau == tenMau);

            if (tenDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.TenMau),
                    "Tên mẫu sản phẩm đã tồn tại. Vui lòng nhập tên khác."
                );
            }
        }

        await ValidateChiTietAsync(model);

        if (!ModelState.IsValid)
        {
            await NapDanhSachAsync(model, true);
            return View("Create", model);
        }

        var image = await LuuAnhAsync(model.TepAnh);

        var entity = new MauSanPham
        {
            TenMau = tenMau,
            MaDip = model.MaDip,
            KieuSanPham = model.KieuSanPham,
            GiaCoBan = model.GiaCoBan,
            HinhAnh = image ?? model.HinhAnh,
            MoTa = model.MoTa,
            TrangThai = model.TrangThai,
            NgayTao = DateTime.Now
        };

        foreach (var item in model.ChiTietMaus.Where(x => x.MaHoa > 0))
        {
            entity.ChiTietMaus.Add(new ChiTietMau
            {
                MaHoa = item.MaHoa,
                SoLuongTieuChuan = item.SoLuongTieuChuan,
                GhiChu = item.GhiChu
            });
        }

        _context.MauSanPhams.Add(entity);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm mẫu sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _context.MauSanPhams.Include(m => m.ChiTietMaus).FirstOrDefaultAsync(m => m.MaMau == id);
        if (entity == null) return NotFound();
        var model = new MauSanPhamFormViewModel
        {
            MaMau = entity.MaMau,
            TenMau = entity.TenMau,
            MaDip = entity.MaDip,
            KieuSanPham = entity.KieuSanPham,
            GiaCoBan = entity.GiaCoBan,
            HinhAnh = entity.HinhAnh,
            MoTa = entity.MoTa,
            TrangThai = entity.TrangThai,
            ChiTietMaus = entity.ChiTietMaus.Select(c => new ChiTietMauFormItem
            {
                MaChiTietMau = c.MaChiTietMau,
                MaHoa = c.MaHoa,
                SoLuongTieuChuan = c.SoLuongTieuChuan,
                GhiChu = c.GhiChu
            }).ToList()
        };
        await NapDanhSachAsync(model, true);
        return View("Edit", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MauSanPhamFormViewModel model)
    {
        var entity = await _context.MauSanPhams
            .Include(m => m.ChiTietMaus)
            .FirstOrDefaultAsync(m => m.MaMau == model.MaMau);

        if (entity == null)
        {
            TempData["Error"] = "Không tìm thấy mẫu sản phẩm cần cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra dữ liệu một lần duy nhất
        await ValidateChiTietAsync(model);

        string tenMau = model.TenMau?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenMau))
        {
            ModelState.AddModelError(
                nameof(model.TenMau),
                "Vui lòng nhập tên mẫu sản phẩm."
            );
        }
        else
        {
            bool tenDaTonTai = await _context.MauSanPhams
                .AnyAsync(x => x.TenMau == tenMau && x.MaMau != id);

            if (tenDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.TenMau),
                    "Tên mẫu sản phẩm đã tồn tại. Vui lòng nhập tên khác."
                );
            }
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(x => x.Value!.Errors)
                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
                    ? "Dữ liệu nhập không hợp lệ."
                    : x.ErrorMessage)
                .Distinct()
                .ToList();

            ViewBag.LoiCapNhat = errors;

            await NapDanhSachAsync(model, true);
            return View("Edit", model);
        }

        try
        {
            // Cập nhật thông tin mẫu
            entity.TenMau = model.TenMau.Trim();
            entity.MaDip = model.MaDip;
            entity.KieuSanPham = model.KieuSanPham;
            entity.GiaCoBan = model.GiaCoBan;
            entity.MoTa = model.MoTa;
            entity.TrangThai = model.TrangThai;

            // Không chọn ảnh mới thì giữ ảnh hiện tại
            var anhMoi = await LuuAnhAsync(model.TepAnh);
            if (!string.IsNullOrWhiteSpace(anhMoi))
            {
                entity.HinhAnh = anhMoi;
            }

            // Cập nhật thành phần hoa
            _context.ChiTietMaus.RemoveRange(entity.ChiTietMaus.ToList());

            foreach (var item in model.ChiTietMaus.Where(x => x.MaHoa > 0))
            {
                entity.ChiTietMaus.Add(new ChiTietMau
                {
                    MaHoa = item.MaHoa,
                    SoLuongTieuChuan = item.SoLuongTieuChuan,
                    GhiChu = item.GhiChu
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật mẫu sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            // Ghi log chi tiết ngoại lệ ở đây nếu project có ILogger.
            ModelState.AddModelError(
                string.Empty,
                "Không thể cập nhật mẫu sản phẩm. Vui lòng kiểm tra dữ liệu và cơ sở dữ liệu.");

            await NapDanhSachAsync(model, true);
            return View("Edit", model);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _context.MauSanPhams
            .AsNoTracking()
            .Include(m => m.DipSuDung)
            .Include(m => m.ChiTietMaus)
                .ThenInclude(c => c.HoaTuoi)
            .FirstOrDefaultAsync(m => m.MaMau == id);

        if (entity == null)
            return NotFound();

        var model = new MauSanPhamFormViewModel
        {
            MaMau = entity.MaMau,
            TenMau = entity.TenMau,
            MaDip = entity.MaDip,
            KieuSanPham = entity.KieuSanPham,
            GiaCoBan = entity.GiaCoBan,
            HinhAnh = entity.HinhAnh,
            MoTa = entity.MoTa,
            TrangThai = entity.TrangThai,

            ChiTietMaus = entity.ChiTietMaus.Select(c =>
                new ChiTietMauFormItem
                {
                    MaChiTietMau = c.MaChiTietMau,
                    MaHoa = c.MaHoa,
                    SoLuongTieuChuan = c.SoLuongTieuChuan,
                    GhiChu = c.GhiChu
                }).ToList(),

            DanhSachHoa = await _context.HoaTuois
                .AsNoTracking()
                .Where(h => h.TrangThai)
                .OrderBy(h => h.TenHoa)
                .Select(h => new SelectListItem
                {
                    Value = h.MaHoa.ToString(),
                    Text = h.TenHoa + " (" + h.DonViTinh + ")"
                })
                .ToListAsync()
        };

        ViewBag.TenDip = entity.DipSuDung?.TenDip;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteConfirm(int id)
    {
        var mau = await _context.MauSanPhams
            .AsNoTracking()
            .Include(m => m.DipSuDung)
            .FirstOrDefaultAsync(m => m.MaMau == id);

        if (mau == null)
            return NotFound();

        return View("Delete", mau);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mau = await _context.MauSanPhams
            .FirstOrDefaultAsync(m => m.MaMau == id);

        if (mau == null)
        {
            TempData["Error"] = "Không tìm thấy mẫu sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // Xóa mềm: giữ nguyên mẫu và chi tiết thành phần
        mau.TrangThai = false;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Mẫu sản phẩm đã được chuyển sang trạng thái ngừng bán.";
        return RedirectToAction(nameof(Index));
    }


    private async Task NapDanhSachAsync(
        MauSanPhamFormViewModel model,
        bool chiLayDipHoatDong)
    {
        ViewBag.DanhSachDip = await _context.DipSuDungs
            .Where(d => d.TrangThai)
            .OrderBy(d => d.TenDip)
            .ToListAsync();

        model.DanhSachHoa = await _context.HoaTuois
            .AsNoTracking()
            .Where(h => h.TrangThai)
            .OrderBy(h => h.TenHoa)
            .Select(h => new SelectListItem
            {
                Value = h.MaHoa.ToString(),
                Text = h.TenHoa + "|" + h.DonViTinh + "|" + h.DonGiaThamKhao
            })
            .ToListAsync();

        if (model.ChiTietMaus == null)
            model.ChiTietMaus = new List<ChiTietMauFormItem>();

        if (model.ChiTietMaus.Count == 0)
            model.ChiTietMaus.Add(new ChiTietMauFormItem());
    }


    private async Task ValidateChiTietAsync(MauSanPhamFormViewModel model)
    {
        var rows = model.ChiTietMaus
            .Where(x => x.MaHoa > 0)
            .ToList();

        // Không cho cùng một loại hoa xuất hiện hai lần
        if (rows.GroupBy(x => x.MaHoa).Any(g => g.Count() > 1))
        {
            ModelState.AddModelError(
                nameof(model.ChiTietMaus),
                "Một loại hoa không được xuất hiện nhiều lần trong cùng mẫu.");
        }

        foreach (var row in rows)
        {
            if (row.SoLuongTieuChuan <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.ChiTietMaus),
                    "Số lượng tiêu chuẩn phải lớn hơn 0.");
            }

            var hoaHopLe = await _context.HoaTuois
                .AnyAsync(h => h.MaHoa == row.MaHoa && h.TrangThai);

            if (!hoaHopLe)
            {
                ModelState.AddModelError(
                    nameof(model.ChiTietMaus),
                    "Loại hoa không tồn tại hoặc đã ngừng hoạt động.");
            }
        }

        var dipHopLe = await _context.DipSuDungs
            .AnyAsync(d => d.MaDip == model.MaDip && d.TrangThai);

        if (!dipHopLe)
        {
            ModelState.AddModelError(
                nameof(model.MaDip),
                "Dịp sử dụng không tồn tại hoặc đã ngừng hoạt động.");
        }
    }

    private async Task<string?> LuuAnhAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.");
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Ảnh không được vượt quá 5 MB.");
        var folder = Path.Combine(_env.WebRootPath, "images", "mau");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream);
        return $"/images/mau/{name}";
    }
}
