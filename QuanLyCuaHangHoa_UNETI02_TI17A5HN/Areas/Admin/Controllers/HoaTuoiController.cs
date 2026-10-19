// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Quản lý hoa tươi, tìm kiếm, lọc, sắp xếp, phân trang và kiểm tra dữ liệu khi thêm, sửa, xóa

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using System.Globalization;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class HoaTuoiController : Controller
    {
        private readonly AppDbContext _context;

        public HoaTuoiController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? loaiHoaId,
            string? mauSac,
            string? donViTinh,
            string? trangThai,
            string? sapXep,
            int page = 1)
        {
            // =====================================================
            // 4 Ô THỐNG KÊ - LẤY DỮ LIỆU THẬT TỪ DATABASE
            // =====================================================

            // Tổng số hoa tươi
            ViewBag.TongLoaiHoa = await _context.HoaTuois
                .AsNoTracking()
                .CountAsync();

            // Số hoa đang sử dụng
            ViewBag.DangSuDung = await _context.HoaTuois
                .AsNoTracking()
                .CountAsync(h => h.TrangThai);

            // Số hoa tạm ngừng
            ViewBag.TamNgungNhap = await _context.HoaTuois
                .AsNoTracking()
                .CountAsync(h => !h.TrangThai);

            // Giá tham khảo trung bình của toàn bộ hoa tươi
            ViewBag.GiaThamKhaoTB = await _context.HoaTuois
                .AsNoTracking()
                .Select(h => (decimal?)h.DonGiaThamKhao)
                .AverageAsync() ?? 0m;

            // Lấy dữ liệu hoa tươi và thông tin loại hoa từ database
            IQueryable<HoaTuoi> query = _context.HoaTuois
                .AsNoTracking()
                .Include(h => h.LoaiHoa);


            // Tìm kiếm tổng hợp theo mã, tên, giá, màu sắc, đơn vị tính, loại hoa và mô tả
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                // Hỗ trợ tìm mã dạng HT-001, HT-1, 001 hoặc 1
                string maHoaTimKiem = tuKhoa;

                if (maHoaTimKiem.StartsWith(
                    "HT-", StringComparison.OrdinalIgnoreCase))
                {
                    maHoaTimKiem = maHoaTimKiem.Substring(3);
                }

                int? maHoa = int.TryParse(
                    maHoaTimKiem,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int maHoaValue)
                    ? maHoaValue
                    : null;

                // Hỗ trợ nhập giá như 50000 hoặc 50.000
                decimal? donGia = decimal.TryParse(
                    tuKhoa,
                    NumberStyles.Number,
                    CultureInfo.GetCultureInfo("vi-VN"),
                    out decimal donGiaValue)
                    ? donGiaValue
                    : null;

                query = query.Where(h =>
                    // Mã hoa
                    (maHoa.HasValue && h.MaHoa == maHoa.Value)

                    // Tên hoa
                    || h.TenHoa.Contains(tuKhoa)

                    // Màu sắc
                    || h.MauSac.Contains(tuKhoa)

                    // Đơn vị tính
                    || h.DonViTinh.Contains(tuKhoa)

                    // Loại hoa
                    || h.LoaiHoa.TenLoaiHoa.Contains(tuKhoa)

                    // Mô tả
                    || (h.MoTa != null && h.MoTa.Contains(tuKhoa))

                    // Đơn giá chính xác
                    || (donGia.HasValue
                        && h.DonGiaThamKhao == donGia.Value)
                );
            }

            // Lọc theo loại hoa
            if (loaiHoaId.HasValue)
            {
                query = query.Where(h => h.MaLoaiHoa == loaiHoaId.Value);
            }

            // Lọc theo màu sắc
            if (!string.IsNullOrWhiteSpace(mauSac))
            {
                query = query.Where(h => h.MauSac == mauSac);
            }

            // Lọc theo đơn vị tính
            if (!string.IsNullOrWhiteSpace(donViTinh))
            {
                query = query.Where(h => h.DonViTinh == donViTinh);
            }

            // Lọc theo trạng thái
            if (trangThai == "true" || trangThai == "HoatDong")
            {
                query = query.Where(h => h.TrangThai);
            }
            else if (trangThai == "false" ||
                     trangThai == "NgungHoatDong")
            {
                query = query.Where(h => !h.TrangThai);
            }


            // Sắp xếp
            switch (sapXep?.Trim().ToLowerInvariant())
            {
                case "name_asc":
                case "ten_asc":
                    query = query.OrderBy(h => h.TenHoa)
                                 .ThenBy(h => h.MaHoa);
                    break;

                case "name_desc":
                case "ten_desc":
                    query = query.OrderByDescending(h => h.TenHoa)
                                 .ThenBy(h => h.MaHoa);
                    break;

                case "gia_asc":
                    query = query.OrderBy(h => h.DonGiaThamKhao)
                                 .ThenBy(h => h.MaHoa);
                    break;

                case "gia_desc":
                    query = query.OrderByDescending(h => h.DonGiaThamKhao)
                                 .ThenBy(h => h.MaHoa);
                    break;

                case "date_new":
                case "ma_moi":
                    query = query.OrderByDescending(h => h.MaHoa);
                    break;

                case "date_old":
                case "ma_cu":
                    query = query.OrderBy(h => h.MaHoa);
                    break;

                default:
                    query = query.OrderBy(h => h.TenHoa)
                                 .ThenBy(h => h.MaHoa);
                    break;
            }

            // Phân trang
            var danhSachHoa = await PagedList<HoaTuoi>.CreateAsync(
                query, page, 10);

            // Đổ dữ liệu vào ViewModel
            var model = new HoaTuoiListViewModel
            {
                DanhSachHoa = danhSachHoa,
                TuKhoa = tuKhoa,
                LoaiHoaId = loaiHoaId,
                TrangThai = trangThai,
                SapXep = sapXep,
                MauSac = mauSac,
                DonViTinh = donViTinh,
                TrangHienTai = danhSachHoa.TrangHienTai,
                SoBanGhiMoiTrang = danhSachHoa.SoBanGhiMoiTrang,

                DanhSachMauSac = await _context.HoaTuois
                    .AsNoTracking()
                    .Select(h => h.MauSac)
                    .Distinct()
                    .OrderBy(m => m)
                    .Select(m => new SelectListItem
                    {
                        Value = m,
                        Text = m
                    })
                    .ToListAsync(),

                DanhSachDonViTinh = await _context.HoaTuois
                    .AsNoTracking()
                    .Select(h => h.DonViTinh)
                    .Distinct()
                    .OrderBy(d => d)
                    .Select(d => new SelectListItem
                    {
                        Value = d,
                        Text = d
                    })
                    .ToListAsync(),

                DanhSachLoaiHoa = await _context.LoaiHoas
                    .AsNoTracking()
                    .OrderBy(l => l.TenLoaiHoa)
                    .Select(l => new SelectListItem
                    {
                        Value = l.MaLoaiHoa.ToString(),
                        Text = l.TenLoaiHoa
                    })
                    .ToListAsync()
            };

            // Dữ liệu cho thanh lọc dùng chung của project
            ViewData["AdminFilter"] = new AdminFilterViewModel
            {
                Area = "Admin",
                Controller = "HoaTuoi",
                Action = "Index",
                TuKhoa = tuKhoa,
                TrangThai = trangThai == "true" ||
                            trangThai == "HoatDong" ? true :
                            trangThai == "false" ||
                            trangThai == "NgungHoatDong" ? false : null,
                SapXep = string.IsNullOrWhiteSpace(sapXep)
                    ? "name_asc"
                    : sapXep,

                HienThiBoLocHoaTuoi = true,

                LoaiHoaId = loaiHoaId,
                DanhSachLoaiHoa = model.DanhSachLoaiHoa,

                MauSac = mauSac,
                DanhSachMauSac = model.DanhSachMauSac,

                DonViTinh = donViTinh,
                DanhSachDonViTinh = model.DanhSachDonViTinh,

                Placeholder = "Tìm theo mã, tên, màu sắc, loại hoa...",
                SapXepOptions = new List<AdminSortOption>
                {
                    new() { Value = "name_asc", Text = "Tên hoa (A-Z)" },
                    new() { Value = "name_desc", Text = "Tên hoa (Z-A)" },
                    new() { Value = "date_new", Text = "Mã hoa mới nhất" },
                    new() { Value = "date_old", Text = "Mã hoa cũ nhất" },
                    new() { Value = "gia_asc", Text = "Giá tăng dần" },
                    new() { Value = "gia_desc", Text = "Giá giảm dần" }
                }
            };

            // Dữ liệu cho phân trang dùng chung
            ViewData["Pagination"] = new PaginationViewModel
            {
                Area = "Admin",
                Controller = "HoaTuoi",
                Action = "Index",
                TrangHienTai = danhSachHoa.TrangHienTai,
                TongSoTrang = danhSachHoa.TongSoTrang,
                TongSoBanGhi = danhSachHoa.TongSoBanGhi,
                SoBanGhiMoiTrang = danhSachHoa.SoBanGhiMoiTrang,
                BoLoc = new Dictionary<string, string?>
                {
                    ["tuKhoa"] = tuKhoa,
                    ["loaiHoaId"] = loaiHoaId?.ToString(),
                    ["mauSac"] = mauSac,
                    ["donViTinh"] = donViTinh,
                    ["trangThai"] = trangThai,
                    ["sapXep"] = sapXep
                }
            };

            return View(model);
        }

        // ================================================================
        // POST: /Admin/HoaTuoi/Create - Thêm hoa tươi
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string tenHoa,
            int maLoaiHoa,
            string mauSac,
            string donViTinh,
            decimal donGiaThamKhao,
            string? moTa,
            bool trangThai = true)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(tenHoa) ||
                string.IsNullOrWhiteSpace(mauSac) ||
                string.IsNullOrWhiteSpace(donViTinh))
            {
                TempData["Loi"] = "Vui lòng nhập đầy đủ tên hoa, màu sắc và đơn vị tính.";
                return RedirectToAction(nameof(Index));
            }

            if (tenHoa.Trim().Length > 100 ||
                mauSac.Trim().Length > 50 ||
                donViTinh.Trim().Length > 30 ||
                (moTa?.Length ?? 0) > 1000)
            {
                TempData["Loi"] = "Dữ liệu vượt quá độ dài cho phép.";
                return RedirectToAction(nameof(Index));
            }

            if (donGiaThamKhao < 0)
            {
                TempData["Loi"] = "Đơn giá tham khảo không được âm.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng tên hoa, không phân biệt chữ hoa/chữ thường
            string tenHoaChuanHoa = tenHoa.Trim();

            bool trungTen = await _context.HoaTuois
                .AnyAsync(h => h.TenHoa.Trim().ToLower()
                    == tenHoaChuanHoa.ToLower());

            if (trungTen)
            {
                TempData["Error"] =
                    $"Tên hoa \"{tenHoaChuanHoa}\" đã tồn tại. Vui lòng nhập tên hoa khác.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra loại hoa tồn tại và đang hoạt động
            var loaiHoaHopLe = await _context.LoaiHoas.AnyAsync(
                l => l.MaLoaiHoa == maLoaiHoa && l.TrangThai);

            if (!loaiHoaHopLe)
            {
                TempData["Loi"] = "Loại hoa không tồn tại hoặc đã ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            var hoa = new HoaTuoi
            {
                TenHoa = tenHoa.Trim(),
                MaLoaiHoa = maLoaiHoa,
                MauSac = mauSac.Trim(),
                DonViTinh = donViTinh.Trim(),
                DonGiaThamKhao = donGiaThamKhao,
                MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim(),
                TrangThai = trangThai
            };

            _context.HoaTuois.Add(hoa);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã thêm hoa '{hoa.TenHoa}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/HoaTuoi/Edit - Sửa thông tin hoa tươi
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int maHoa,
            string tenHoa,
            int maLoaiHoa,
            string mauSac,
            string donViTinh,
            decimal donGiaThamKhao,
            string? moTa,
            bool trangThai)
        {
            var hoa = await _context.HoaTuois
                .FirstOrDefaultAsync(h => h.MaHoa == maHoa);

            if (hoa == null)
            {
                TempData["Loi"] = "Không tìm thấy hoa tươi cần sửa.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(tenHoa) ||
                string.IsNullOrWhiteSpace(mauSac) ||
                string.IsNullOrWhiteSpace(donViTinh))
            {
                TempData["Loi"] = "Vui lòng nhập đầy đủ tên hoa, màu sắc và đơn vị tính.";
                return RedirectToAction(nameof(Index));
            }

            if (tenHoa.Trim().Length > 100 ||
                mauSac.Trim().Length > 50 ||
                donViTinh.Trim().Length > 30 ||
                (moTa?.Length ?? 0) > 1000)
            {
                TempData["Loi"] = "Dữ liệu vượt quá độ dài cho phép.";
                return RedirectToAction(nameof(Index));
            }

            if (donGiaThamKhao < 0)
            {
                TempData["Loi"] = "Đơn giá tham khảo không được âm.";
                return RedirectToAction(nameof(Index));
            }

            var loaiHoaTonTai = await _context.LoaiHoas
                .AnyAsync(l => l.MaLoaiHoa == maLoaiHoa);

            if (!loaiHoaTonTai)
            {
                TempData["Loi"] = "Loại hoa được chọn không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra tên hoa có bị trùng với bản ghi khác không
            string tenHoaChuanHoa = tenHoa.Trim();

            bool trungTen = await _context.HoaTuois
                .AnyAsync(h => h.MaHoa != maHoa
                    && h.TenHoa.Trim().ToLower()
                        == tenHoaChuanHoa.ToLower());

            if (trungTen)
            {
                TempData["Error"] =
                    $"Tên hoa \"{tenHoaChuanHoa}\" đã được sử dụng bởi một hoa tươi khác. Vui lòng nhập tên khác.";

                return RedirectToAction(nameof(Index));
            }

            // Cập nhật các trường được phép sửa
            hoa.TenHoa = tenHoaChuanHoa;
            hoa.MaLoaiHoa = maLoaiHoa;
            hoa.MauSac = mauSac.Trim();
            hoa.DonViTinh = donViTinh.Trim();
            hoa.DonGiaThamKhao = donGiaThamKhao;
            hoa.MoTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
            hoa.TrangThai = trangThai;

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã cập nhật hoa '{hoa.TenHoa}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // POST: /Admin/HoaTuoi/Delete - Xóa mềm hoa tươi
        // ================================================================


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hoa = await _context.HoaTuois
                .FirstOrDefaultAsync(h => h.MaHoa == id);

            if (hoa == null)
            {
                TempData["Error"] = "Không tìm thấy hoa tươi cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra hoa có đang được mẫu hoa sử dụng không
            bool coMauHoaThamChieu = await _context.ChiTietMaus
                .AnyAsync(ct => ct.MaHoa == id);

            if (coMauHoaThamChieu)
            {
                TempData["Error"] =
                    $"Không thể xóa hoa \"{hoa.TenHoa}\" (mã HT-{hoa.MaHoa:D3}) " +
                    "vì hoa đang được sử dụng trong mẫu hoa. " +
                    "Vui lòng kiểm tra các mẫu hoa liên quan.";

                return RedirectToAction(nameof(Index));
            }

            // Thử xóa hoa nếu không có mẫu hoa tham chiếu
            _context.HoaTuois.Remove(hoa);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Đã xóa hoa \"{hoa.TenHoa}\" (mã HT-{hoa.MaHoa:D3}) thành công.";
            }
            catch (DbUpdateException)
            {
                // Database từ chối xóa do còn dữ liệu liên quan
                // Không tự động xóa các bản ghi ở bảng khác
                TempData["Error"] =
                    $"Không thể xóa hoa \"{hoa.TenHoa}\" (mã HT-{hoa.MaHoa:D3}) " +
                    "vì vẫn còn dữ liệu ở bảng khác đang tham chiếu đến hoa hoặc mẫu hoa. " +
                    "Dữ liệu hiện tại được giữ nguyên.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
