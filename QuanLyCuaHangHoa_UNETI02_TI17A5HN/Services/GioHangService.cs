// Họ và tên: Đặng Thị Mai Hương
// Mã sinh viên: 23103100293
// Nội dung thực hiện: Dùng chung Session giỏ hàng giữa Cart và Checkout
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

public class GioHangService
{
    private readonly AppDbContext _db;
    public GioHangService(AppDbContext db) => _db = db;

    // Mỗi tài khoản dùng một khóa Session riêng để tránh lẫn giỏ giữa các lần đăng nhập.
    public static string Key(int maTaiKhoan) => $"GioHang_{maTaiKhoan}";

    // Chỉ lấy mã mẫu và số lượng hợp lệ từ Session; không lưu giá bán trong giỏ.
    public List<CartItem> Read(ISession session, int account) =>
        (session.GetJson<List<CartItem>>(Key(account)) ?? new())
        .Where(x => x.MaMau > 0 && x.SoLuong > 0).ToList();

    // Lưu thay đổi giỏ sau thao tác thêm/sửa/xóa.
    public void Save(ISession session, int account, List<CartItem> items) => session.SetJson(Key(account), items);

    // Xóa giỏ của tài khoản sau khi đặt đơn thành công hoặc khách chủ động xóa.
    public void Clear(ISession session, int account) => session.Remove(Key(account));

    // Ghép thông tin từ Session với dữ liệu DB để giao diện luôn dùng tên, ảnh, giá hiện tại.
    public async Task<List<CartDisplayItem>> DisplayAsync(ISession session, int account)
    {
        var cart = Read(session, account);
        var ids = cart.Select(x => x.MaMau).Distinct().ToList();
        // Truy vấn tất cả mẫu trong giỏ một lần, không truy vấn DB cho từng dòng.
        var products = await _db.MauSanPhams.AsNoTracking().Where(x => ids.Contains(x.MaMau)).ToDictionaryAsync(x => x.MaMau);
        // Mẫu đã bị xóa/ngừng bán vẫn được hiển thị để khách biết và tự loại khỏi giỏ.
        return cart.Select(x => new CartDisplayItem
        {
            MaMau = x.MaMau,
            SoLuong = x.SoLuong,
            GiayGoiRuyBang = x.GiayGoiRuyBang,
            LoiChuc = x.LoiChuc,
            TenMau = products.TryGetValue(x.MaMau, out var prod) ? prod.TenMau : "Mẫu hoa không còn tồn tại",
            HinhAnh = products.TryGetValue(x.MaMau, out var img) ? img.HinhAnh : null,
            DonGia = products.TryGetValue(x.MaMau, out var price) ? price.GiaCoBan : 0,
            ConBan = products.TryGetValue(x.MaMau, out var live) && live.TrangThai
        }).ToList();
    }
}
