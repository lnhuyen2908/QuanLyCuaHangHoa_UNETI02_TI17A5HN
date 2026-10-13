// Họ và tên: Nguyễn Thị Phương Anh
// Mã sinh viên: 23103100267
// Nội dung thực hiện: Phân trang Skip/Take dùng chung

using Microsoft.EntityFrameworkCore;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers
{
    public class PagedList<T>
    {
        public List<T> DuLieu { get; set; } = new();

        public int TrangHienTai { get; set; }

        public int SoBanGhiMoiTrang { get; set; }

        public int TongSoBanGhi { get; set; }

        public int TongSoTrang => (int)Math.Ceiling((double)TongSoBanGhi / SoBanGhiMoiTrang);

        public bool HasPrev => TrangHienTai > 1;

        public bool HasNext => TrangHienTai < TongSoTrang;

        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> query,
            int trangHienTai = 1,
            int soBanGhiMoiTrang = 10)
        {
            if (trangHienTai < 1)
                trangHienTai = 1;

            if (soBanGhiMoiTrang < 1)
                soBanGhiMoiTrang = 10;

            // Đếm tổng số bản ghi sau khi tìm kiếm/lọc
            int tongSoBanGhi = await query.CountAsync();

            int tongSoTrang = (int)Math.Ceiling(
                (double)tongSoBanGhi / soBanGhiMoiTrang);

            // Đưa trang hiện tại về phạm vi hợp lệ
            if (tongSoTrang == 0)
                trangHienTai = 1;
            else if (trangHienTai > tongSoTrang)
                trangHienTai = tongSoTrang;

            // Chỉ lấy dữ liệu của trang cần hiển thị
            var duLieu = await query
                .Skip((trangHienTai - 1) * soBanGhiMoiTrang)
                .Take(soBanGhiMoiTrang)
                .ToListAsync();

            return new PagedList<T>
            {
                DuLieu = duLieu,
                TrangHienTai = trangHienTai,
                SoBanGhiMoiTrang = soBanGhiMoiTrang,
                TongSoBanGhi = tongSoBanGhi
            };
        }
    }
}
