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
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Session vẫn hoạt động khi chưa đồng ý cookie
            });

            builder.Services.AddScoped<TaiKhoanService>();

            var app = builder.Build();

            // Migrate + seed (tài khoản admin/nhanvien01/... mật khẩu 123456)
            await SeedData.InitializeAsync(app.Services);

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
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