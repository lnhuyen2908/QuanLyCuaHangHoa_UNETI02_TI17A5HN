// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Quản lý mẫu sản phẩm, thành phần hoa, upload nhiều ảnh, tìm kiếm, lọc, sắp xếp và phân trang.

using ClosedXML.Excel;
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

    public async Task<IActionResult> Index(string? tuKhoa, int? loaiHoaId, int? maDip, string? kieuSanPham,
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
                m.KieuSanPham.Contains(key) ||
                m.ChiTietMaus.Any(c => c.HoaTuoi.TenHoa.Contains(key) ||  c.HoaTuoi.MauSac.Contains(key)));
        }
        if (maDip.HasValue) query = query.Where(m => m.MaDip == maDip.Value);
        if (loaiHoaId.HasValue)
            query = query.Where(m => m.ChiTietMaus.Any(c => c.HoaTuoi.MaLoaiHoa == loaiHoaId.Value));
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
            "moi_nhat" => query.OrderByDescending(m => m.NgayTao).ThenByDescending(m => m.MaMau),
            "cu_nhat" => query.OrderBy(m => m.NgayTao).ThenBy(m => m.MaMau),
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
            Placeholder = "Tìm tên mẫu, tên hoa, màu sắc, dịp, kiểu sản phẩm...",
            HienThiTrangThai = true,

            SapXepOptions = new List<AdminSortOption>
            {
                new() { Value = "", Text = "Mã mẫu tăng dần" },
                new() { Value = "ten_asc", Text = "Tên A-Z" },
                new() { Value = "ten_desc", Text = "Tên Z-A" },
                new() { Value = "gia_asc", Text = "Giá tăng dần" },
                new() { Value = "gia_desc", Text = "Giá giảm dần" },
                new() { Value = "moi_nhat", Text = "Mới nhất" },
                new() { Value = "cu_nhat", Text = "Cũ nhất" }
            },

            LoaiHoaId = loaiHoaId,
            DanhSachLoaiHoa = await _context.LoaiHoas.AsNoTracking()
                .OrderBy(l => l.TenLoaiHoa)
                .Select(l => new SelectListItem(l.TenLoaiHoa, l.MaLoaiHoa.ToString()))
                .ToListAsync(),

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
                ["loaiHoaId"] = loaiHoaId?.ToString(),
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

        var dsAnhPhu = await LuuNhieuAnhAsync(model.DanhSachAnh, nameof(model.DanhSachAnh));

        string? image = null;
        try
        {
            image = await LuuAnhAsync(model.TepAnh);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.TepAnh), ex.Message);
        }

        if (model.TepAnh == null || model.TepAnh.Length == 0)
        {
            ModelState.AddModelError(nameof(model.TepAnh), "Vui lòng chọn ảnh đại diện cho mẫu sản phẩm.");
        }

        if (!ModelState.IsValid)
        {
            await NapDanhSachAsync(model, true);
            return View("Create", model);
        }

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

        int thuTu = 1;
        foreach (var duongDan in dsAnhPhu)
        {
            entity.AnhMauSanPhams.Add(new AnhMauSanPham { DuongDan = duongDan, ThuTu = thuTu++ });
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
            .Include(m => m.AnhMauSanPhams)
            .FirstOrDefaultAsync(m => m.MaMau == model.MaMau);

        if (entity == null)
        {
            TempData["Error"] = "Không tìm thấy mẫu sản phẩm cần cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra dữ liệu một lần duy nhất
        await ValidateChiTietAsync(model);

        int soAnhConLai = entity.AnhMauSanPhams.Count(a => model.XoaAnhIds == null || !model.XoaAnhIds.Contains(a.MaAnh));
        var dsAnhPhu = await LuuNhieuAnhAsync(model.DanhSachAnh, nameof(model.DanhSachAnh), soAnhConLai);

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

        // Sau khi lưu, mẫu phải còn ảnh đại diện: hoặc giữ ảnh cũ, hoặc chọn ảnh mới
        bool conAnhDaiDien = !string.IsNullOrWhiteSpace(entity.HinhAnh) && !model.XoaAnhChinh;
        if (!conAnhDaiDien && (model.TepAnh == null || model.TepAnh.Length == 0))
        {
            ModelState.AddModelError(nameof(model.TepAnh), "Vui lòng chọn ảnh đại diện cho mẫu sản phẩm.");
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
            entity.TenMau =     model.TenMau.Trim();
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
            else if (model.XoaAnhChinh && !string.IsNullOrWhiteSpace(entity.HinhAnh))
            {
                var duongDanCu = entity.HinhAnh;
                entity.HinhAnh = null;

                // Chỉ xóa file thật nếu không còn mẫu nào khác dùng chung ảnh này
                bool mauKhacDung = await _context.MauSanPhams
                    .AnyAsync(m => m.MaMau != entity.MaMau && m.HinhAnh == duongDanCu);

                if (!mauKhacDung)
                {
                    var fileVatLy = Path.Combine(_env.WebRootPath,
                        duongDanCu.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fileVatLy)) System.IO.File.Delete(fileVatLy);
                }
            }

            // Xóa ảnh phụ được chọn
            if (model.XoaAnhIds != null && model.XoaAnhIds.Count > 0)
            {
                var anhCanXoa = entity.AnhMauSanPhams
                    .Where(a => model.XoaAnhIds.Contains(a.MaAnh)).ToList();

                foreach (var anh in anhCanXoa)
                {
                    var fileVatLy = Path.Combine(_env.WebRootPath,
                        anh.DuongDan.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fileVatLy)) System.IO.File.Delete(fileVatLy);
                }
                _context.AnhMauSanPhams.RemoveRange(anhCanXoa);
            }

            // Thêm ảnh phụ mới
            int thuTu = entity.AnhMauSanPhams.Any() ? entity.AnhMauSanPhams.Max(a => a.ThuTu) + 1 : 1;
            foreach (var duongDan in dsAnhPhu)
            {
                entity.AnhMauSanPhams.Add(new AnhMauSanPham { DuongDan = duongDan, ThuTu = thuTu++ });
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
            .Include(m => m.AnhMauSanPhams)
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
            AnhHienCo = entity.AnhMauSanPhams.OrderBy(a => a.ThuTu).ToList(),

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
            .Include(m => m.ChiTietMaus).ThenInclude(c => c.HoaTuoi)
            .Include(m => m.AnhMauSanPhams)
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

        if (model.MaMau > 0)
        {
            model.AnhHienCo = await _context.AnhMauSanPhams
                .AsNoTracking()
                .Where(a => a.MaMau == model.MaMau)
                .OrderBy(a => a.ThuTu)
                .ToListAsync();
        }
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

    private async Task<List<string>> LuuNhieuAnhAsync(List<IFormFile>? files, string tenTruong, int soAnhDaCo = 0)
    {
        var ketQua = new List<string>();
        if (files == null) return ketQua;

        var hopLe = files.Where(f => f != null && f.Length > 0).ToList();
        if (soAnhDaCo + hopLe.Count > 5)
        {
            ModelState.AddModelError(tenTruong, "Mỗi mẫu chỉ được tối đa 5 ảnh phụ.");
            return ketQua;
        }

        foreach (var f in hopLe)
        {
            try
            {
                var duongDan = await LuuAnhAsync(f);
                if (duongDan != null) ketQua.Add(duongDan);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(tenTruong, ex.Message);
            }
        }
        return ketQua;
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


    // ================================================================
    // GET: /Admin/MauSanPham/ExportExcel
    // Xuất danh sách mẫu sản phẩm ra file Excel
    // Bao gồm cả các hoa thành phần và số lượng tiêu chuẩn
    // ================================================================
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        // BƯỚC 1: Lấy danh sách mẫu sản phẩm từ database
        // Include DipSuDung để lấy tên dịp sử dụng
        // ThenInclude HoaTuoi để lấy các loại hoa thành phần

        var danhSachMau = await _context.MauSanPhams
            .AsNoTracking()
            .Include(m => m.DipSuDung)
            .Include(m => m.ChiTietMaus)
                .ThenInclude(ct => ct.HoaTuoi)
            .OrderBy(m => m.MaMau)
            .ToListAsync();

        // BƯỚC 2: Tạo Workbook
        using var workbook = new XLWorkbook();

        // BƯỚC 3: Tạo Worksheet
        var worksheet = workbook.Worksheets.Add("DanhSachMauSanPham");

        // BƯỚC 4: Tạo tiêu đề
        worksheet.Cell(1, 1).Value = "DANH SÁCH MẪU SẢN PHẨM";

        // Gộp các ô từ A1 đến I1
        worksheet.Range("A1:I1").Merge();

        var title = worksheet.Range("A1:I1");

        title.Style.Font.Bold = true;
        title.Style.Font.FontSize = 16;
        title.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        title.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Row(1).Height = 30;

        // BƯỚC 5: Tạo tiêu đề các cột
        string[] headers =
        {
            "Mã mẫu",
            "Tên mẫu sản phẩm",
            "Dịp sử dụng",
            "Kiểu sản phẩm",
            "Giá cơ bản",
            "Hoa thành phần",
            "Số lượng tiêu chuẩn",
            "Ngày tạo",
            "Trạng thái"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(3, i + 1).Value = headers[i];
        }

        // Định dạng hàng tiêu đề
        var header = worksheet.Range("A3:I3");

        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.DarkGreen;

        header.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        header.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        header.Style.Alignment.WrapText = true;

        header.Style.Border.OutsideBorder =
            XLBorderStyleValues.Thin;

        header.Style.Border.InsideBorder =
            XLBorderStyleValues.Thin;

        worksheet.Row(3).Height = 35;

        // BƯỚC 6: Đổ dữ liệu mẫu sản phẩm vào Excel
        int row = 4;

        foreach (var mau in danhSachMau)
        {
            // Lấy danh sách tên hoa thành phần
            var danhSachHoa = mau.ChiTietMaus
                .Where(ct => ct.HoaTuoi != null)
                .Select(ct => ct.HoaTuoi.TenHoa)
                .ToList();

            // Lấy số lượng tương ứng của từng loại hoa
            var danhSachSoLuong = mau.ChiTietMaus
                .Where(ct => ct.HoaTuoi != null)
                .Select(ct => ct.SoLuongTieuChuan.ToString())
                .ToList();

            // Mã mẫu
            worksheet.Cell(row, 1).Value = mau.MaMau;

            // Tên mẫu
            worksheet.Cell(row, 2).Value = mau.TenMau;

            // Dịp sử dụng
            worksheet.Cell(row, 3).Value =
                mau.DipSuDung?.TenDip ?? "";

            // Kiểu sản phẩm
            worksheet.Cell(row, 4).Value = mau.KieuSanPham;

            // Giá cơ bản
            worksheet.Cell(row, 5).Value = mau.GiaCoBan;

            // Tên các hoa thành phần
            worksheet.Cell(row, 6).Value =
                danhSachHoa.Count > 0
                    ? string.Join("\n", danhSachHoa)
                    : "Chưa có hoa thành phần";

            // Số lượng tiêu chuẩn
            worksheet.Cell(row, 7).Value =
                danhSachSoLuong.Count > 0
                    ? string.Join("\n", danhSachSoLuong)
                    : "";

            // Ngày tạo
            worksheet.Cell(row, 8).Value = mau.NgayTao;

            // Trạng thái
            worksheet.Cell(row, 9).Value =
                mau.TrangThai ? "Đang mở bán" : "Tạm ngừng bán";

            row++;
        }

        // BƯỚC 7: Định dạng dữ liệu

        // Định dạng tiền Việt Nam
        worksheet.Column(5).Style.NumberFormat.Format = "#,##0";

        // Định dạng ngày tháng
        worksheet.Column(8).Style.DateFormat.Format = "dd/MM/yyyy";

        // Căn giữa mã mẫu, số lượng và trạng thái
        worksheet.Column(1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(7).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(9).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        // Cho phép xuống dòng trong cột hoa thành phần
        worksheet.Column(6).Style.Alignment.WrapText = true;
        worksheet.Column(7).Style.Alignment.WrapText = true;

        // Kẻ đường viền dữ liệu
        if (row > 4)
        {
            var dataRange = worksheet.Range(4, 1, row - 1, 9);

            dataRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            dataRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            dataRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }

        // BƯỚC 8: Tạo bộ lọc
        worksheet.Range(3, 1, Math.Max(3, row - 1), 9)
            .SetAutoFilter();

        // Cố định 3 hàng đầu
        worksheet.SheetView.FreezeRows(3);

        // Tự động điều chỉnh độ rộng các cột
        worksheet.Columns().AdjustToContents();

        // Giới hạn độ rộng một số cột dài
        worksheet.Column(2).Width = 28;
        worksheet.Column(6).Width = 30;
        worksheet.Column(7).Width = 22;

        // BƯỚC 9: Lưu file Excel vào bộ nhớ
        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        // BƯỚC 10: Trả file Excel về trình duyệt
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"DanhSachMauSanPham_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
    }

}
