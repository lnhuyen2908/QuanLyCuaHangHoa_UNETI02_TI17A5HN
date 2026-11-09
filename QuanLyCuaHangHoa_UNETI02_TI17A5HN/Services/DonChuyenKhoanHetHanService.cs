// NGUYỄN THỊ ĐÀO - 23103100245 - TỰ HỦY ĐƠN SAU 24H - LƯU LY DO HỦY VÀ LS XỬ LÝ -  
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    /// <summary>
    /// Tự động hủy đơn chuyển khoản nếu quá 24 giờ
    /// kể từ lúc đặt mà chưa được ghi nhận thanh toán.
    /// </summary>
    public class DonChuyenKhoanHetHanService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DonChuyenKhoanHetHanService> _logger;

        public DonChuyenKhoanHetHanService(
            IServiceScopeFactory scopeFactory,
            ILogger<DonChuyenKhoanHetHanService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await HuyDonQuaHanAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Lỗi khi tự động hủy đơn chuyển khoản quá hạn.");
                }

                // Kiểm tra lại sau mỗi phút.
                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }

        private async Task HuyDonQuaHanAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            // Thời điểm đặt đơn phải cách hiện tại ít nhất 24 giờ.
            var mocHetHan = DateTime.Now.AddHours(-24);

            // Chỉ lấy đơn chuyển khoản chưa thanh toán,
            // đang ở trạng thái chờ xác nhận và đã quá 24 giờ.
            var dsDonQuaHan = await db.DonDatHoas
                .Where(d =>
                    d.TrangThai == TrangThaiDon.ChoXacNhan
                    && d.GhiChu != null
                    && d.GhiChu.StartsWith("[PTTT=CHUYENKHOAN]")
                    && d.NgayDat <= mocHetHan
                    && !db.ThanhToans.Any(t =>
                        t.MaDon == d.MaDon))
                .Select(d => d.MaDon)
                .ToListAsync(cancellationToken);

            foreach (var maDon in dsDonQuaHan)
            {
                // Kiểm tra lại điều kiện ngay lúc cập nhật
                // để tránh hủy đơn vừa được ghi nhận thanh toán.
                var soDonDaHuy = await db.DonDatHoas
                    .Where(d =>
                        d.MaDon == maDon
                        && d.TrangThai == TrangThaiDon.ChoXacNhan
                        && d.GhiChu != null
                        && d.GhiChu.StartsWith("[PTTT=CHUYENKHOAN]")
                        && d.NgayDat <= mocHetHan
                        && !db.ThanhToans.Any(t =>
                            t.MaDon == d.MaDon))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            d => d.TrangThai,
                            TrangThaiDon.DaHuy)
                        .SetProperty(
                            d => d.LyDoHuy,
                            "Tự động hủy do quá 24 giờ kể từ khi đặt hàng nhưng chưa thanh toán chuyển khoản."),
                        cancellationToken);

                if (soDonDaHuy > 0)
                {
                    _logger.LogInformation(
                        "Đã tự động hủy đơn hàng {MaDon} do quá 24 giờ chưa thanh toán.",
                        maDon);
                }
            }
        }
    }
}
