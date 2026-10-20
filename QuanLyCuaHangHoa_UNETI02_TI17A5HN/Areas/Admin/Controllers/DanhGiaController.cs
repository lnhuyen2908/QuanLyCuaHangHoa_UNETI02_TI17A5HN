
// Họ và tên: Nguyễn Thị Đào
// Mã sinh viên: 23103100245
// Nội dung: Tuần 6 - Quản lý đánh giá,
// tính điểm trung bình, thống kê sao bằng LINQ.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]
    public class DanhGiaController : Controller
    {
        private readonly AppDbContext _db;

        public DanhGiaController(AppDbContext db)
        {
            _db = db;
        }

        // Nguyễn Thị Đào - Danh sách và thống kê đánh giá
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string sapXep = "date_new",
            int page = 1)
        {
            // Thống kê từ toàn bộ đánh giá.
            var thongKe = await _db.DanhGias
                .AsNoTracking()
                .GroupBy(d => d.SoSao)
                .Select(g => new
                {
                    SoSao = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            var vm = new DanhGiaAdminViewModel
            {
                TuKhoa = tuKhoa,
                SapXep = sapXep
            };

            foreach (var item in thongKe)
            {
                if (item.SoSao >= 1 && item.SoSao <= 5)
                    vm.ThongKeSao[item.SoSao] = item.SoLuong;
            }

            vm.TongDanhGia = vm.ThongKeSao.Sum();

            if (vm.TongDanhGia > 0)
            {
                double tongDiem = thongKe.Sum(
                    x => (double)x.SoSao * x.SoLuong);

                vm.DiemTrungBinh = Math.Round(
                    tongDiem / vm.TongDanhGia, 2);

                vm.TyLe5Sao = Math.Round(
                    vm.ThongKeSao[5] * 100.0 / vm.TongDanhGia, 1);
            }

            // Truy vấn danh sách đánh giá.
            var query = _db.DanhGias
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.DonDatHoa)
                .AsQueryable();

            // Tìm theo mã đơn, tên khách hoặc nhận xét.
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                string maNhap = tuKhoa.TrimStart('#');

                if (maNhap.StartsWith(
                    "ORD-", StringComparison.OrdinalIgnoreCase))
                {
                    maNhap = maNhap.Substring(4);
                }

                if (int.TryParse(maNhap, out int maDon))
                {
                    query = query.Where(d => d.MaDon == maDon);
                }
                else
                {
                    query = query.Where(d =>
                        d.KhachHang.HoTen.Contains(tuKhoa) ||
                        (d.NoiDung != null &&
                         d.NoiDung.Contains(tuKhoa)));
                }
            }

            // Sắp xếp.
            query = sapXep switch
            {
                "date_old" => query
                    .OrderBy(d => d.NgayDanhGia),

                "rate_high" => query
                    .OrderByDescending(d => d.SoSao)
                    .ThenByDescending(d => d.NgayDanhGia),

                "rate_low" => query
                    .OrderBy(d => d.SoSao)
                    .ThenByDescending(d => d.NgayDanhGia),

                _ => query
                    .OrderByDescending(d => d.NgayDanhGia)
            };

            // Tái sử dụng phân trang của nhóm.
            vm.DanhSach = await PagedList<
                QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities.DanhGia
            >.CreateAsync(query, page, 10);

            return View(vm);
        }
    }
}
