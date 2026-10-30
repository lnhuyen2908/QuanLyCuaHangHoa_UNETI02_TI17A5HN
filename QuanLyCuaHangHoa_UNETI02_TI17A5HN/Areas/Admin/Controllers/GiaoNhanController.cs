
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - Quản lý giao nhận, tìm kiếm,
// lọc, sắp xếp và phân trang bằng LINQ.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;
// Bổ sung thư viện xử lý lỗi trùng dữ liệu SQL Server.
using Microsoft.Data.SqlClient;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class GiaoNhanController : Controller
    {
        private readonly AppDbContext _db;

        public GiaoNhanController(AppDbContext db)
        {
            _db = db;
        }

        // Nguyễn Thị Đào - Danh sách giao nhận
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiGiaoNhan,
            string sapXep = "date_new",
            int page = 1)
        {
            var query = _db.GiaoNhans
                .Include(g => g.DonDatHoa)
                .AsNoTracking()
                .AsQueryable();

            // Tìm theo mã đơn, mã giao nhận, tên người nhận,
            // địa chỉ và người giao.

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                string maNhap = tuKhoa.TrimStart('#').Trim();

                bool timMaDon = maNhap.StartsWith(
                    "ORD-", StringComparison.OrdinalIgnoreCase);

                bool timMaGiaoNhan = maNhap.StartsWith(
                    "GN-", StringComparison.OrdinalIgnoreCase);

                if (timMaDon)
                    maNhap = maNhap.Substring(4);
                else if (timMaGiaoNhan)
                    maNhap = maNhap.Substring(3);

                if (int.TryParse(maNhap, out int ma))
                {
                    if (timMaDon)
                    {
                        query = query.Where(g => g.MaDon == ma);
                    }
                    else if (timMaGiaoNhan)
                    {
                        query = query.Where(g => g.MaGiaoNhan == ma);
                    }
                    else
                    {
                        query = query.Where(g =>
                            g.MaDon == ma || g.MaGiaoNhan == ma);
                    }
                }
                else
                {
                    query = query.Where(g =>
                        (g.DiaChi != null &&
                         g.DiaChi.Contains(tuKhoa)) ||

                        (g.NguoiGiao != null &&
                         g.NguoiGiao.Contains(tuKhoa)) ||

                        g.DonDatHoa.NguoiNhan.Contains(tuKhoa));
                }
            }

            // Lọc trạng thái
            if (!string.IsNullOrWhiteSpace(trangThaiGiaoNhan))
            {
                query = query.Where(g =>
                    g.TrangThai == trangThaiGiaoNhan);
            }

            // Sắp xếp
            query = sapXep switch
            {
                "date_old" => query
                    .OrderBy(g => g.ThoiGianDuKien),

                "status" => query
                    .OrderBy(g => g.TrangThai)
                    .ThenByDescending(g => g.ThoiGianDuKien),

                _ => query
                    .OrderByDescending(g => g.ThoiGianDuKien)
            };

            // Phân trang bằng lớp dùng chung
            var danhSach = await PagedList<GiaoNhan>
                .CreateAsync(query, page, 10);

            // Thống kê LINQ từ SQL Server
            var vm = new GiaoNhanIndexViewModel
            {
                TuKhoa = tuKhoa,
                TrangThaiGiaoNhan = trangThaiGiaoNhan,
                SapXep = sapXep,
                DanhSach = danhSach,

                TongGiaoNhan = await _db.GiaoNhans.CountAsync(),

                ChoXuLy = await _db.GiaoNhans.CountAsync(
                    g => g.TrangThai == TrangThaiGiaoNhanConst.ChoXuLy),

                DangGiao = await _db.GiaoNhans.CountAsync(
                    g => g.TrangThai == TrangThaiGiaoNhanConst.DangGiao),

                HoanThanh = await _db.GiaoNhans.CountAsync(
                    g => g.TrangThai == TrangThaiGiaoNhanConst.HoanThanh)
            };

            return View(vm);
        }


        // Nội dung: Tuần 6 - Thêm phiếu giao nhận.
        // Lấy các đơn đủ điều kiện và chưa có phiếu giao nhận.
        private async Task NapDanhSachDonAsync(
            GiaoNhanCreateViewModel vm)
        {
            var ds = await _db.DonDatHoas
                .AsNoTracking()
                .Where(d =>
                    (d.TrangThai == TrangThaiDon.DaXacNhan ||
                     d.TrangThai == TrangThaiDon.DangChuanBi ||
                     d.TrangThai == TrangThaiDon.SanSang)
                    && !_db.GiaoNhans.Any(g => g.MaDon == d.MaDon))
                .OrderByDescending(d => d.MaDon)
                .Select(d => new
                {
                    d.MaDon,
                    d.NguoiNhan,
                    d.HinhThucNhan
                })
                .ToListAsync();

            vm.DonHopLe = ds.Select(d =>
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.MaDon.ToString(),
                    Text = $"#ORD-{d.MaDon:D4} - {d.NguoiNhan} - {d.HinhThucNhan}",
                    Selected = d.MaDon == vm.MaDon
                }).ToList();
        }

        // GET: Admin/GiaoNhan/Create/ chỉ mở form, tải ds đơn đủ đk, điền sẵn dữ liệu
        [HttpGet]
        public async Task<IActionResult> Create(int? maDon)
        {
            var vm = new GiaoNhanCreateViewModel
            {
                MaDon = maDon ?? 0 
            };

            if (maDon.HasValue)
            {
                var don = await _db.DonDatHoas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaDon == maDon.Value);

                if (don != null)
                {
                    vm.DiaChi = don.DiaChiGiao;

                    if (don.ThoiGianNhanGiao > DateTime.Now)
                        vm.ThoiGianDuKien = don.ThoiGianNhanGiao;
                }
            }

            await NapDanhSachDonAsync(vm); // kiểm tra lại điều kiện

            return View(vm);
        }

        // POST: Admin/GiaoNhan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GiaoNhanCreateViewModel vm)
        {
            var don = await _db.DonDatHoas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDon == vm.MaDon);

            if (don == null)
            {
                ModelState.AddModelError(
                    nameof(vm.MaDon),
                    "Đơn đặt hoa không tồn tại.");
            }
            else
            {
                // Đơn phải nằm trong luồng xử lý hợp lệ.
                if (don.TrangThai != TrangThaiDon.DaXacNhan &&
                    don.TrangThai != TrangThaiDon.DangChuanBi &&
                    don.TrangThai != TrangThaiDon.SanSang)
                {
                    ModelState.AddModelError(
                        nameof(vm.MaDon),
                        "Đơn chưa đủ điều kiện lập phiếu giao nhận.");
                }

                // Mỗi đơn chỉ có một phiếu giao nhận.
                bool daCoPhieu = await _db.GiaoNhans
                    .AnyAsync(g => g.MaDon == vm.MaDon);

                if (daCoPhieu)
                {
                    ModelState.AddModelError(
                        nameof(vm.MaDon),
                        "Đơn này đã có phiếu giao nhận.");
                }

                // Kiểm tra thời gian dự kiến.
                if (vm.ThoiGianDuKien <
                    don.NgayDat.AddMinutes(30))
                {
                    ModelState.AddModelError(
                        nameof(vm.ThoiGianDuKien),
                        "Thời gian dự kiến phải cách lúc đặt ít nhất 30 phút.");
                }

                if (vm.ThoiGianDuKien <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(vm.ThoiGianDuKien),
                        "Thời gian dự kiến phải ở tương lai.");
                }

                // Kiểm tra thông tin giao tận nơi.
                if (don.HinhThucNhan ==
                    HinhThucNhanConst.GiaoTanNoi)
                {
                    var diaChi = string.IsNullOrWhiteSpace(vm.DiaChi)
                        ? don.DiaChiGiao
                        : vm.DiaChi;

                    if (string.IsNullOrWhiteSpace(diaChi))
                    {
                        ModelState.AddModelError(
                            nameof(vm.DiaChi),
                            "Giao tận nơi bắt buộc có địa chỉ.");
                    }

                    if (string.IsNullOrWhiteSpace(don.NguoiNhan) ||
                        string.IsNullOrWhiteSpace(don.SoDienThoaiNguoiNhan))
                    {
                        ModelState.AddModelError(
                            nameof(vm.MaDon),
                            "Đơn thiếu thông tin người nhận.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachDonAsync(vm);
                return View(vm);
            }

            // Tạo Entity từ dữ liệu đã được kiểm tra.
            var giaoNhan = new GiaoNhan
            {
                MaDon = vm.MaDon,

                // Không lấy hình thức từ form./ đã đc k.h lựa chọn khi đặt hàng r
                HinhThuc = don!.HinhThucNhan,

                ThoiGianDuKien = vm.ThoiGianDuKien,
                ThoiGianThucTe = null,

                DiaChi = don.HinhThucNhan ==
                    HinhThucNhanConst.GiaoTanNoi
                    ? (string.IsNullOrWhiteSpace(vm.DiaChi)
                        ? don.DiaChiGiao?.Trim()
                        : vm.DiaChi.Trim())
                    : null,

                NguoiGiao = vm.NguoiGiao?.Trim(),
                GhiChu = vm.GhiChu?.Trim(),

                // Phiếu mới luôn chờ xử lý.
                TrangThai = TrangThaiGiaoNhanConst.ChoXuLy
            };

            // Nội dung: Xử lý trường hợp hai nhân viên
            // cùng tạo phiếu giao nhận cho một đơn.
            try
            {
                // Thêm phiếu giao nhận vào Database.
                _db.GiaoNhans.Add(giaoNhan);

                await _db.SaveChangesAsync();

                TempData["Success"] =
                    "Tạo phiếu giao nhận thành công.";
            }
            
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx &&
                      (sqlEx.Number == 2601 ||  // lỗi trùng
                       sqlEx.Number == 2627))   // lỗi vi phạm ràng buộc UNIQUE và PRIMARY
            {
                // SQL Server phát hiện trùng Unique Index.
                TempData["Error"] =
                    "Đơn này đã có phiếu giao nhận. " +
                    "Có thể nhân viên khác vừa tạo phiếu.";

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));

        }
        // KẾT THÚC PHẦN CREATE


        // Nội dung: Tuần 6 - Sửa phiếu giao nhận (Edit)
        // GET: Admin/GiaoNhan/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var giaoNhan = await _db.GiaoNhans
                .Include(g => g.DonDatHoa)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiaoNhan == id);

            if (giaoNhan == null)
                return NotFound();

            // Chỉ sửa phiếu chờ xử lý.
            if (giaoNhan.TrangThai !=
                TrangThaiGiaoNhanConst.ChoXuLy)
            {
                TempData["Error"] =
                    "Chỉ được sửa phiếu đang chờ xử lý.";

                return RedirectToAction(nameof(Index));
            }

            var don = giaoNhan.DonDatHoa;

            if (don.TrangThai != TrangThaiDon.DaXacNhan &&
                don.TrangThai != TrangThaiDon.DangChuanBi &&
                don.TrangThai != TrangThaiDon.SanSang)
            {
                TempData["Error"] =
                    "Đơn không còn ở trạng thái cho phép sửa giao nhận.";

                return RedirectToAction(nameof(Index));
            }

            var vm = new GiaoNhanEditViewModel
            {
                MaGiaoNhan = giaoNhan.MaGiaoNhan,
                MaDon = giaoNhan.MaDon,
                TenNguoiNhan = don.NguoiNhan,
                HinhThuc = giaoNhan.HinhThuc,
                ThoiGianDuKien = giaoNhan.ThoiGianDuKien,
                DiaChi = giaoNhan.DiaChi,
                NguoiGiao = giaoNhan.NguoiGiao,
                GhiChu = giaoNhan.GhiChu
            };

            return View(vm);
        }

        // POST: Admin/GiaoNhan/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( int id, GiaoNhanEditViewModel vm)
        {
            if (id != vm.MaGiaoNhan)
                return BadRequest();

            var giaoNhan = await _db.GiaoNhans
                .Include(g => g.DonDatHoa)
                .FirstOrDefaultAsync(g => g.MaGiaoNhan == id);

            if (giaoNhan == null)
                return NotFound();

            if (giaoNhan.TrangThai !=
                TrangThaiGiaoNhanConst.ChoXuLy)
            {
                TempData["Error"] =
                    "Phiếu đã bắt đầu giao hoặc hoàn thành, không thể sửa.";

                return RedirectToAction(nameof(Index));
            }

            var don = giaoNhan.DonDatHoa;

            if (don.TrangThai != TrangThaiDon.DaXacNhan &&
                don.TrangThai != TrangThaiDon.DangChuanBi &&
                don.TrangThai != TrangThaiDon.SanSang)
            {
                TempData["Error"] =
                    "Đơn không còn ở trạng thái cho phép sửa.";

                return RedirectToAction(nameof(Index));
            }

            // Không cho đổi hình thức của đơn.
            if (giaoNhan.HinhThuc != don.HinhThucNhan)
            {
                TempData["Error"] =
                    "Hình thức giao nhận không khớp với đơn.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra thời gian dự kiến.
            if (vm.ThoiGianDuKien <
                don.NgayDat.AddMinutes(30))
            {
                ModelState.AddModelError(
                    nameof(vm.ThoiGianDuKien),
                    "Thời gian dự kiến phải cách lúc đặt ít nhất 30 phút.");
            }

            if (vm.ThoiGianDuKien <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(vm.ThoiGianDuKien),
                    "Thời gian dự kiến mới phải ở tương lai.");
            }

            var diaChiMoi = string.IsNullOrWhiteSpace(vm.DiaChi)
                ? don.DiaChiGiao
                : vm.DiaChi.Trim();

            if (don.HinhThucNhan ==
                HinhThucNhanConst.GiaoTanNoi)
            {
                if (string.IsNullOrWhiteSpace(diaChiMoi))
                {
                    ModelState.AddModelError(
                        nameof(vm.DiaChi),
                        "Giao tận nơi bắt buộc có địa chỉ.");
                }

                if (string.IsNullOrWhiteSpace(don.NguoiNhan) ||
                    string.IsNullOrWhiteSpace(don.SoDienThoaiNguoiNhan))
                {
                    ModelState.AddModelError(
                        nameof(vm.MaGiaoNhan),
                        "Đơn thiếu thông tin người nhận.");
                }
            }

            // Giữ lại dữ liệu hiển thị nếu validation lỗi.
            vm.MaDon = don.MaDon;
            vm.TenNguoiNhan = don.NguoiNhan;
            vm.HinhThuc = don.HinhThucNhan;

            if (!ModelState.IsValid)
                return View(vm);

            // Chỉ cập nhật các trường được phép sửa.
            giaoNhan.ThoiGianDuKien = vm.ThoiGianDuKien;

            giaoNhan.DiaChi =
                don.HinhThucNhan == HinhThucNhanConst.GiaoTanNoi
                    ? diaChiMoi!.Trim()
                    : null;

            giaoNhan.NguoiGiao = vm.NguoiGiao?.Trim();
            giaoNhan.GhiChu = vm.GhiChu?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật phiếu giao nhận thành công.";

            return RedirectToAction(nameof(Index));
        }
        // KẾT THÚC PHẦN EDIT 

        // Nội dung: Tuần 6 - Hoàn thành giao nhận
        // Đồng bộ trạng thái đơn và lưu lịch sử xử lý.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            if (id <= 0)
                return NotFound();

            // Lấy người đang thực hiện từ Session.
            var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
                return Unauthorized();

            // Lấy phiếu và trạng thái đơn tương ứng.
            var phieu = await _db.GiaoNhans
                .AsNoTracking()
                .Where(g => g.MaGiaoNhan == id)
                .Select(g => new
                {
                    g.MaDon,
                    g.HinhThuc,
                    g.TrangThai,
                    TrangThaiDonHienTai = g.DonDatHoa.TrangThai,
                    HinhThucNhanDon = g.DonDatHoa.HinhThucNhan
                })
                .FirstOrDefaultAsync();

            if (phieu == null)
                return NotFound();

            // Ngăn hoàn thành trùng lặp.
            if (phieu.TrangThai ==
                TrangThaiGiaoNhanConst.HoanThanh)
            {
                TempData["Error"] =
                    "Phiếu giao nhận đã hoàn thành trước đó.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra hình thức giao nhận khớp với đơn.
            if (phieu.HinhThuc != phieu.HinhThucNhanDon)
            {
                TempData["Error"] =
                    "Hình thức giao nhận không khớp với đơn.";

                return RedirectToAction(nameof(Index));
            }

            // Giao tận nơi: đơn phải đang giao.
            bool giaoTanNoiHopLe =
                phieu.HinhThuc == HinhThucNhanConst.GiaoTanNoi &&
                phieu.TrangThaiDonHienTai == TrangThaiDon.DangGiao &&
                phieu.TrangThai == TrangThaiGiaoNhanConst.DangGiao;

            // Khách đến nhận: đơn phải sẵn sàng.
            bool khachDenNhanHopLe =
                phieu.HinhThuc == HinhThucNhanConst.KhachDenNhan &&
                phieu.TrangThaiDonHienTai == TrangThaiDon.SanSang &&
                phieu.TrangThai == TrangThaiGiaoNhanConst.ChoXuLy;

            if (!giaoTanNoiHopLe && !khachDenNhanHopLe)
            {
                TempData["Error"] =
                    "Đơn chưa ở trạng thái cho phép hoàn thành giao nhận.";

                return RedirectToAction(nameof(Index));
            }

            // Dùng lại Service đã viết ở tuần 5.
            // Service sẽ kiểm tra thanh toán và điều kiện giao nhận,
            // cập nhật trạng thái đơn, phiếu và ghi lịch sử.
            var service = new TrangThaiDonService(_db);

            var ketQua = await service.ChuyenAsync(
                phieu.MaDon,
                TrangThaiDon.HoanThanh,
                maTaiKhoan.Value,
                "Xác nhận khách đã nhận hoa thành công."
            );

            TempData[ketQua.ThanhCong ? "Success" : "Error"]
                = ketQua.ThongBao;

            return RedirectToAction(nameof(Index));
        }

        // KẾT THÚC PHẦN HOÀN THÀNH GIAO NHẬN 


    }
}
