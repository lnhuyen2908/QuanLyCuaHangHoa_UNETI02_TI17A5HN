using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Session vẫn hoạt động khi chưa đồng ý cookie
            });

            builder.Services.AddScoped<DonHangService>();
            builder.Services.AddScoped<GioHangService>();
            builder.Services.AddScoped<TaiKhoanService>();
            builder.Services.AddHostedService<DonChuyenKhoanHetHanService>();
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Thay TenDbContextCuaBan bằng tên lớp DbContext thực tế của dự án
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi tự động cập nhật Database khi khởi động.");
                }
            }

            // Migrate + seed (tài khoản admin/nhanvien01/... mật khẩu 123456)
            await SeedData.InitializeAsync(app.Services);

            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownIPNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedHeaderOptions);

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Cho phép truy cập ảnh, CSS, JavaScript trong wwwroot
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            // Tự động thu hồi session nếu tài khoản bị khóa trong CSDL
            app.Use(async (context, next) =>
            {
                var session = context.Session;
                if (session.DaDangNhap())
                {
                    var maTaiKhoan = session.GetMaTaiKhoan();
                    if (maTaiKhoan.HasValue)
                    {
                        var taiKhoanService = context.RequestServices.GetRequiredService<TaiKhoanService>();
                        if (!await taiKhoanService.ConHoatDongAsync(maTaiKhoan.Value))
                        {
                            session.Clear();
                        }
                    }
                }
                await next();
            });

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}