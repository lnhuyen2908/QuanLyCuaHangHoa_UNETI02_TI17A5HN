
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Module 1
        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<LoaiHoa> LoaiHoas => Set<LoaiHoa>();
        public DbSet<DipSuDung> DipSuDungs => Set<DipSuDung>();

        // Module 2
        public DbSet<HoaTuoi> HoaTuois => Set<HoaTuoi>();
        public DbSet<MauSanPham> MauSanPhams => Set<MauSanPham>();
        public DbSet<ChiTietMau> ChiTietMaus => Set<ChiTietMau>();

        // Module 3
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<DonDatHoa> DonDatHoas => Set<DonDatHoa>();
        public DbSet<ChiTietDonHoa> ChiTietDonHoas => Set<ChiTietDonHoa>();

        // Module 4
        public DbSet<LichSuXuLyDon> LichSuXuLyDons => Set<LichSuXuLyDon>();
        public DbSet<GiaoNhan> GiaoNhans => Set<GiaoNhan>();
        public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
        public DbSet<DanhGia> DanhGias => Set<DanhGia>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ===== Module 1 =====
            // TaiKhoan (1) - (0..1) KhachHang
            mb.Entity<KhachHang>()
              .HasOne(k => k.TaiKhoan)
              .WithOne(t => t.KhachHang)
              .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== Module 2 =====
            // LoaiHoa (1) - (n) HoaTuoi
            mb.Entity<HoaTuoi>()
              .HasOne(h => h.LoaiHoa)
              .WithMany(l => l.HoaTuois)
              .HasForeignKey(h => h.MaLoaiHoa)
              .OnDelete(DeleteBehavior.Restrict);

            // DipSuDung (1) - (n) MauSanPham
            mb.Entity<MauSanPham>()
              .HasOne(m => m.DipSuDung)
              .WithMany(d => d.MauSanPhams)
              .HasForeignKey(m => m.MaDip)
              .OnDelete(DeleteBehavior.Restrict);

            // MauSanPham (1) - (n) ChiTietMau
            mb.Entity<ChiTietMau>()
              .HasOne(c => c.MauSanPham)
              .WithMany(m => m.ChiTietMaus)
              .HasForeignKey(c => c.MaMau)
              .OnDelete(DeleteBehavior.Cascade);

            // HoaTuoi (1) - (n) ChiTietMau
            mb.Entity<ChiTietMau>()
              .HasOne(c => c.HoaTuoi)
              .WithMany(h => h.ChiTietMaus)
              .HasForeignKey(c => c.MaHoa)
              .OnDelete(DeleteBehavior.Restrict);

            // Một loại hoa không lặp nhiều dòng trong cùng mẫu
            mb.Entity<ChiTietMau>()
              .HasIndex(c => new { c.MaMau, c.MaHoa })
              .IsUnique();

            // ===== Module 3 =====
            // KhachHang (1) - (n) DonDatHoa (Restrict để bảo toàn lịch sử)
            mb.Entity<DonDatHoa>()
              .HasOne(d => d.KhachHang)
              .WithMany(k => k.DonDatHoas)
              .HasForeignKey(d => d.MaKhachHang)
              .OnDelete(DeleteBehavior.Restrict);

            // DonDatHoa (1) - (n) ChiTietDonHoa
            mb.Entity<ChiTietDonHoa>()
              .HasOne(c => c.DonDatHoa)
              .WithMany(d => d.ChiTietDonHoas)
              .HasForeignKey(c => c.MaDon)
              .OnDelete(DeleteBehavior.Cascade);

            // MauSanPham (1) - (n) ChiTietDonHoa (MaMau nullable cho thiết kế riêng)
            mb.Entity<ChiTietDonHoa>()
              .HasOne(c => c.MauSanPham)
              .WithMany()
              .HasForeignKey(c => c.MaMau)
              .IsRequired(false)
              .OnDelete(DeleteBehavior.Restrict);

            // ===== Module 4 =====
            // DonDatHoa (1) - (n) LichSuXuLyDon
            mb.Entity<LichSuXuLyDon>()
              .HasOne(l => l.DonDatHoa)
              .WithMany(d => d.LichSuXuLyDons)
              .HasForeignKey(l => l.MaDon)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<LichSuXuLyDon>()
              .HasOne<TaiKhoan>()
              .WithMany()
              .HasForeignKey(l => l.NguoiThucHien)
              .OnDelete(DeleteBehavior.Restrict);

            // DonDatHoa (1) - (n) GiaoNhan
            mb.Entity<GiaoNhan>()
              .HasOne(g => g.DonDatHoa)
              .WithMany(d => d.GiaoNhans)
              .HasForeignKey(g => g.MaDon)
              .OnDelete(DeleteBehavior.Restrict);

            // DonDatHoa (1) - (0..n) ThanhToan
            mb.Entity<ThanhToan>()
              .HasOne(t => t.DonDatHoa)
              .WithMany(d => d.ThanhToans)
              .HasForeignKey(t => t.MaDon)
              .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ThanhToan>()
              .HasOne<TaiKhoan>()
              .WithMany()
              .HasForeignKey(t => t.NguoiGhiNhan)
              .OnDelete(DeleteBehavior.Restrict);

            // DonDatHoa (1) - (0..1) DanhGia  -> MaDon unique = mỗi đơn tối đa 1 đánh giá
            mb.Entity<DanhGia>()
              .HasOne(d => d.DonDatHoa)
              .WithOne(o => o.DanhGia)
              .HasForeignKey<DanhGia>(d => d.MaDon)
              .OnDelete(DeleteBehavior.Restrict);

            // KhachHang (1) - (n) DanhGia (Restrict tránh multiple cascade paths)
            mb.Entity<DanhGia>()
              .HasOne(d => d.KhachHang)
              .WithMany()
              .HasForeignKey(d => d.MaKhachHang)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}