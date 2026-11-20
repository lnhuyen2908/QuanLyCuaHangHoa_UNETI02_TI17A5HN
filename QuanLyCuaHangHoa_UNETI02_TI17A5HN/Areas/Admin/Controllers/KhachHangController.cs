// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Danh sách, tìm kiếm, lọc, phân trang, chỉnh sửa và khóa/mở khóa khách hàng.
//đồng bộ thông tin với tài khoản liên kết và kiểm tra ràng buộc đơn hàng.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using ClosedXML.Excel;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class KhachHangController : Controller
    {
        private readonly AppDbContext _context;

        public KhachHangController(AppDbContext context)
        {
            _context = context;
        }

        // Chuẩn hóa lựa chọn sắp xếp
        private string ChuanHoaSapXep(string? sapXep)
        {
            return sapXep switch
            {
                "name_asc" => "name_asc",
                "name_desc" => "name_desc",
                "date_old" => "date_old",
                _ => "date_new"
            };
        }

        // Truy vấn khách hàng theo bộ lọc
        private IQueryable<KhachHang> TaoQueryKhachHang(
            string? tuKhoa,
            bool? trangThai,
            string? sapXep)
        {
            var query = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsNoTracking()
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoa) ||
                    k.SoDienThoai.Contains(tuKhoa) ||
                    k.Email.Contains(tuKhoa));
            }

            // Lọc trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(k =>
                    k.TrangThai == trangThai.Value);
            }

            // Sắp xếp
            query = sapXep switch
            {
                "name_asc" => query.OrderBy(k => k.HoTen)
                                   .ThenBy(k => k.MaKhachHang),

                "name_desc" => query.OrderByDescending(k => k.HoTen)
                                    .ThenBy(k => k.MaKhachHang),

                "date_old" => query.OrderBy(k => k.MaKhachHang),

                _ => query.OrderByDescending(k => k.MaKhachHang)
            };

            return query;
        }

        // Lấy tên trạng thái
        private string LayTenTrangThai(bool? trangThai)
        {
            return trangThai switch
            {
                true => "Đang hoạt động",
                false => "Đã khóa",
                _ => "Tất cả khách hàng"
            };
        }

        // Lấy tên kiểu sắp xếp
        private string LayTenSapXep(string sapXep)
        {
            return sapXep switch
            {
                "name_asc" => "Tên (A-Z)",
                "name_desc" => "Tên (Z-A)",
                "date_old" => "Khách hàng cũ nhất",
                _ => "Khách hàng mới nhất"
            };
        }

        // Tạo tiêu đề báo cáo theo bộ lọc
        private string TaoTieuDeBaoCao(
            string? tuKhoa,
            bool? trangThai)
        {
            string tieuDe = trangThai switch
            {
                true => "DANH SÁCH KHÁCH HÀNG ĐANG HOẠT ĐỘNG",
                false => "DANH SÁCH KHÁCH HÀNG ĐÃ KHÓA",
                _ => "DANH SÁCH TẤT CẢ KHÁCH HÀNG"
            };

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tieuDe += " - KẾT QUẢ TÌM KIẾM";
            }

            return tieuDe;
        }
                
        // GET: Danh sách khách hàng
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai,
                                                string sapXep = "date_new",
                                                int page = 1)
        {
            const int pageSize = 10;

            // Chuẩn hóa sắp xếp
            sapXep = ChuanHoaSapXep(sapXep);

            // Sử dụng truy vấn dùng chung
            var query = TaoQueryKhachHang(
                tuKhoa, trangThai, sapXep);

            // Phân trang
            var danhSach = await PagedList<KhachHang>.CreateAsync(query, page, pageSize);

            return View(new KhachHangIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                SapXep = sapXep,
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                TongDangHoatDong = await _context.KhachHangs.CountAsync(k => k.TrangThai),
                TongBiKhoa = await _context.KhachHangs.CountAsync(k => !k.TrangThai)
            });
        }

        // POST: Sửa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHangEditViewModel model)
        {
            if (id != model.MaKhachHang)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
                return NotFound();

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Khách hàng chưa có tài khoản liên kết hợp lệ!");

                return View(model);
            }

            var hoTen = model.HoTen.Trim();
            var sdt = model.SoDienThoai.Trim();
            var email = model.Email.Trim();

            // Kiểm tra email trùng ở hai bảng
            bool trungEmailKhachHang = await _context.KhachHangs.AnyAsync(k =>
                    k.MaKhachHang != id &&
                    k.Email.ToLower() == email.ToLower());

            bool trungEmailTaiKhoan = await _context.TaiKhoans.AnyAsync(t =>
                    t.MaTaiKhoan != kh.MaTaiKhoan &&
                    t.Email.ToLower() == email.ToLower());

            if (trungEmailKhachHang || trungEmailTaiKhoan)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng trong hệ thống.");
                return View(model);
            }

            // Kiểm tra trùng số điện thoại
            if (await _context.KhachHangs.AnyAsync(k =>
                k.MaKhachHang != id &&
                k.SoDienThoai == sdt))
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại này đã được sử dụng!");
                return View(model);
            }

            // Cập nhật khách hàng
            kh.HoTen = hoTen;
            kh.SoDienThoai = sdt;
            kh.Email = email;
            kh.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();

            // Đồng bộ tài khoản
            kh.TaiKhoan.HoTen = hoTen;
            kh.TaiKhoan.Email = email;

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật khách hàng. Vui lòng thử lại.");
                return View(model);
            }
        }

        // POST: Khóa/mở khóa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
                return NotFound();

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                TempData["ThongBao"] = "Tài khoản liên kết không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trạng thái hai bảng
            if (kh.TrangThai != kh.TaiKhoan.TrangThai)
            {
                TempData["ThongBao"] = "Trạng thái khách hàng và tài khoản không đồng nhất!";
                return RedirectToAction(nameof(Index));
            }

            // Đổi trạng thái cả hai bảng
            bool trangThaiMoi = !kh.TrangThai;

            kh.TrangThai = trangThaiMoi;
            kh.TaiKhoan.TrangThai = trangThaiMoi;

            try
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = trangThaiMoi
                    ? "Mở khóa khách hàng thành công!"
                    : "Khóa khách hàng thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ThongBao"] = "Không thể thay đổi trạng thái khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Thêm khách hàng và tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Create(
            KhachHangCreateViewModel model)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var loi = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage;

                TempData["ThongBao"] = loi ?? "Dữ liệu không hợp lệ!";

                return RedirectToAction(nameof(Index));
            }

            var tenDangNhap = model.TenDangNhap.Trim();
            var hoTen = model.HoTen.Trim();
            var email = model.Email.Trim();
            var sdt = model.SoDienThoai.Trim();

            // Kiểm tra trùng tên đăng nhập
            if (await _context.TaiKhoans.AnyAsync(t =>
                t.TenDangNhap.ToLower() == tenDangNhap.ToLower()))
            {
                TempData["ThongBao"] = $"Tên đăng nhập '{tenDangNhap}' đã tồn tại!";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email trùng ở hai bảng
            if (await _context.TaiKhoans.AnyAsync(t =>
                    t.Email.ToLower() == email.ToLower()) ||
                await _context.KhachHangs.AnyAsync(k =>
                    k.Email.ToLower() == email.ToLower()))
            {
                TempData["ThongBao"] = $"Email '{email}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng số điện thoại
            if (await _context.KhachHangs.AnyAsync(k =>
                k.SoDienThoai == sdt))
            {
                TempData["ThongBao"] = $"Số điện thoại '{sdt}' đã được sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Tạo tài khoản và khách hàng cùng thành công
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tạo tài khoản
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = model.MatKhau,
                    HoTen = hoTen,
                    Email = email,
                    VaiTro = VaiTroConst.KhachHang,
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Tạo hồ sơ khách hàng
                var khachHang = new KhachHang
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = hoTen,
                    SoDienThoai = sdt,
                    Email = email,
                    DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                        ? null
                        : model.DiaChi.Trim(),
                    TrangThai = true
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] = $"Thêm khách hàng '{hoTen}' thành công!";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Không thể tạo khách hàng và tài khoản!";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Đã xảy ra lỗi khi tạo khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa khách hàng và tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTroConst.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Session.CoVaiTro(VaiTroConst.Admin))
                return Forbid();

            var kh = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (kh == null)
            {
                TempData["ThongBao"] = "Không tìm thấy khách hàng cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra tài khoản liên kết
            if (kh.TaiKhoan == null ||
                kh.TaiKhoan.VaiTro != VaiTroConst.KhachHang)
            {
                TempData["ThongBao"] = "Không thể xóa vì tài khoản liên kết không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // Không xóa khách hàng đã có đơn hàng
            bool coDon = await _context.DonDatHoas
                .AnyAsync(d => d.MaKhachHang == id);

            if (coDon)
            {
                TempData["ThongBao"] = $"Khách hàng '{kh.HoTen}' đã có đơn đặt hoa, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Xóa đồng thời hai bảng
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tk = kh.TaiKhoan;

                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();

                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] = $"Đã xóa khách hàng '{kh.HoTen}' và tài khoản liên kết!";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                TempData["ThongBao"] = "Không thể xóa do còn dữ liệu liên kết. Vui lòng sử dụng chức năng Khóa.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Đã xảy ra lỗi khi xóa khách hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Xem trước báo cáo khách hàng
        [HttpGet]
        public async Task<IActionResult> PreviewExcel(
            string? tuKhoa,
            bool? trangThai,
            string sapXep = "date_new")
        {
            sapXep = ChuanHoaSapXep(sapXep);

            // Truy vấn theo đúng bộ lọc
            var danhSach = await TaoQueryKhachHang(
                tuKhoa, trangThai, sapXep)
                .ToListAsync();

            // Không có dữ liệu thì quay lại Index
            if (!danhSach.Any())
            {
                TempData["ThongBao"] =
                    "Không có khách hàng nào phù hợp với điều kiện lọc. Không thể xuất Excel!";

                return RedirectToAction(nameof(Index), new
                {
                    tuKhoa,
                    trangThai,
                    sapXep
                });
            }

            // Thông tin hiển thị trên Preview
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;

            ViewBag.TenTrangThai = LayTenTrangThai(trangThai);
            ViewBag.TenSapXep = LayTenSapXep(sapXep);

            ViewBag.TieuDeBaoCao = TaoTieuDeBaoCao(
                tuKhoa, trangThai);

            return View("PreviewExcel", danhSach);
        }


        // POST: Xuất danh sách khách hàng ra Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatExcel(
            string? tuKhoa,
            bool? trangThai,
            string sapXep = "date_new")
        {
            sapXep = ChuanHoaSapXep(sapXep);

            // 1. Truy vấn lại dữ liệu theo bộ lọc
            var danhSach = await TaoQueryKhachHang(
                tuKhoa, trangThai, sapXep)
                .ToListAsync();

            // 2. Kiểm tra dữ liệu trước khi xuất
            if (!danhSach.Any())
            {
                TempData["ThongBao"] =
                    "Không có khách hàng nào phù hợp với điều kiện lọc. Không thể xuất Excel!";

                return RedirectToAction(nameof(Index), new
                {
                    tuKhoa,
                    trangThai,
                    sapXep
                });
            }

            // 3. Tạo Workbook và Worksheet
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("KhachHang");

            // 4. Tạo tiêu đề báo cáo động
            string tieuDeBaoCao = TaoTieuDeBaoCao(
                tuKhoa, trangThai);

            sheet.Cell(1, 1).Value = tieuDeBaoCao;
            sheet.Range(1, 1, 1, 7).Merge();

            var tieuDe = sheet.Range(1, 1, 1, 7);
            tieuDe.Style.Font.Bold = true;
            tieuDe.Style.Font.FontSize = 16;
            tieuDe.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            tieuDe.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            sheet.Row(1).Height = 30;

            // 5. Tiêu đề phụ
            sheet.Cell(2, 1).Value = "BÁO CÁO QUẢN LÝ KHÁCH HÀNG";
            sheet.Range(2, 1, 2, 7).Merge();

            var tieuDePhu = sheet.Range(2, 1, 2, 7);
            tieuDePhu.Style.Font.Italic = true;
            tieuDePhu.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // 6. Thông tin bộ lọc
            sheet.Cell(4, 1).Value = "THÔNG TIN BỘ LỌC";
            sheet.Range(4, 1, 4, 7).Merge();

            var thongTinLoc = sheet.Range(4, 1, 4, 7);
            thongTinLoc.Style.Font.Bold = true;
            thongTinLoc.Style.Fill.BackgroundColor = XLColor.LightBlue;

            sheet.Cell(5, 1).Value = "Từ khóa:";
            sheet.Cell(5, 2).Value =
                string.IsNullOrWhiteSpace(tuKhoa) ? "Không có" : tuKhoa.Trim();

            sheet.Cell(6, 1).Value = "Trạng thái:";
            sheet.Cell(6, 2).Value = LayTenTrangThai(trangThai);

            sheet.Cell(7, 1).Value = "Sắp xếp:";
            sheet.Cell(7, 2).Value = LayTenSapXep(sapXep);

            sheet.Cell(8, 1).Value = "Ngày xuất:";
            sheet.Cell(8, 2).Value = DateTime.Now;
            sheet.Cell(8, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

            // 7. Tiêu đề các cột dữ liệu
            int dongTieuDe = 10;

            string[] tieuDeCot =
            {
                "Mã khách hàng",
                "Họ và tên",
                "Số điện thoại",
                "Email",
                "Địa chỉ",
                "Tên đăng nhập",
                "Trạng thái"
            };

            for (int cot = 0; cot < tieuDeCot.Length; cot++)
            {
                sheet.Cell(dongTieuDe, cot + 1).Value = tieuDeCot[cot];
            }

            var header = sheet.Range(dongTieuDe, 1, dongTieuDe, 7);

            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightBlue;
            header.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            // 8. Ghi dữ liệu khách hàng
            int dong = dongTieuDe + 1;

            foreach (var kh in danhSach)
            {
                sheet.Cell(dong, 1).Value = $"KH-{kh.MaKhachHang:D3}";
                sheet.Cell(dong, 2).Value = kh.HoTen;
                sheet.Cell(dong, 3).Value = kh.SoDienThoai;
                sheet.Cell(dong, 4).Value = kh.Email;
                sheet.Cell(dong, 5).Value = kh.DiaChi ?? "Chưa cập nhật";

                sheet.Cell(dong, 6).Value =
                    kh.TaiKhoan?.TenDangNhap ?? "Chưa có tài khoản";

                sheet.Cell(dong, 7).Value =
                    kh.TrangThai ? "Hoạt động" : "Đã khóa";

                dong++;
            }

            // 9. Định dạng bảng dữ liệu
            var bang = sheet.Range(
                dongTieuDe, 1,
                dong - 1, 7);

            bang.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            bang.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // Tạo bảng Excel từ dòng dữ liệu
            sheet.Range(
                dongTieuDe + 1, 1,
                dong - 1, 7)
                .CreateTable()
                .Theme = XLTableTheme.TableStyleMedium2;

            // 10. Điều chỉnh độ rộng cột
            sheet.Column(1).Width = 18;
            sheet.Column(2).Width = 25;
            sheet.Column(3).Width = 18;
            sheet.Column(4).Width = 30;
            sheet.Column(5).Width = 35;
            sheet.Column(6).Width = 22;
            sheet.Column(7).Width = 18;

            sheet.Columns().AdjustToContents(1, dong);

            // 11. Cố định dòng tiêu đề
            sheet.SheetView.FreezeRows(dongTieuDe);

            // 12. Lưu Workbook vào MemoryStream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            // 13. Trả file Excel về trình duyệt
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachKhachHang.xlsx"
            );
        }
    }
}

