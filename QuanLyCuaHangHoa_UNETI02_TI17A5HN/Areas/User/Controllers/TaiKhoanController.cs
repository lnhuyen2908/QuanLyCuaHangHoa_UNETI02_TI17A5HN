// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Controller đăng nhập, đăng ký, đăng xuất, đăng nhập mạng xã hội (Gmail & Zalo), trang từ chối truy cập.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services;
using System.Text.Json;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Areas.User.Controllers
{
    [Area("User")]
    [PhanQuyen] // Mặc định phải đăng nhập; các action công khai mở ngoại lệ bằng [AllowAnonymous]
    public class TaiKhoanController : Controller
    {
        private readonly TaiKhoanService _taiKhoanService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public TaiKhoanController(
            TaiKhoanService taiKhoanService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _taiKhoanService = taiKhoanService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // ===================== ĐĂNG NHẬP =====================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (HttpContext.Session.DaDangNhap())
            {
                var maTaiKhoan = HttpContext.Session.GetMaTaiKhoan();
                if (maTaiKhoan.HasValue && await _taiKhoanService.ConHoatDongAsync(maTaiKhoan.Value))
                {
                    return ChuyenHuongTheoVaiTro(HttpContext.Session.GetVaiTro());
                }
                HttpContext.Session.Clear();
            }

            var vm = new LoginViewModel { ReturnUrl = returnUrl };

            // Điền sẵn tên đăng nhập nếu trước đó đã tích "Ghi nhớ"
            var daGhiNho = HttpContext.DocTenDangNhapDaGhiNho();
            if (!string.IsNullOrEmpty(daGhiNho))
            {
                vm.TenDangNhap = daGhiNho;
                vm.GhiNho = true;
            }
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var kq = await _taiKhoanService.DangNhapAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }

            var tk = kq.DuLieu!;
            HttpContext.Session.SetThongTinDangNhap(tk.MaTaiKhoan, tk.HoTen, tk.VaiTro);

            if (vm.GhiNho) HttpContext.GhiNhoTenDangNhap(tk.TenDangNhap);
            else HttpContext.XoaTenDangNhapDaGhiNho();

            return ChuyenHuongSauDangNhap(tk, vm.ReturnUrl);
        }

        // ===================== ĐĂNG KÝ =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.DaDangNhap())
                return ChuyenHuongTheoVaiTro(HttpContext.Session.GetVaiTro());

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var kq = await _taiKhoanService.DangKyAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }

            TempData["ThanhCong"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        // ===================== ĐĂNG NHẬP GMAIL (GOOGLE) =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult LoginGoogle(string? returnUrl = null)
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];

            // Nếu chưa cấu hình Google OAuth trong appsettings.json, chuyển sang trang chọn tài khoản Google giả lập (Demo / Dev Mode)
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                return RedirectToAction(nameof(SocialDemo), new { provider = "Google", returnUrl });
            }

            var redirectUri = Url.Action(nameof(GoogleCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);
            var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl ?? "/"));
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? string.Empty)}" +
                $"&response_type=code" +
                $"&scope=openid%20profile%20email" +
                $"&state={Uri.EscapeDataString(state)}" +
                $"&prompt=select_account";

            return Redirect(googleAuthUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? code, string? state, string? error)
        {
            string? returnUrl = null;
            if (!string.IsNullOrEmpty(state))
            {
                try
                {
                    returnUrl = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                TempData["Loi"] = "Đăng nhập Google thất bại hoặc bị hủy.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];
            var redirectUri = Url.Action(nameof(GoogleCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);

            try
            {
                var client = _httpClientFactory.CreateClient();
                // 1. Đổi authorization code lấy Access Token
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "code", code },
                        { "client_id", clientId! },
                        { "client_secret", clientSecret! },
                        { "redirect_uri", redirectUri! },
                        { "grant_type", "authorization_code" }
                    })
                };

                var tokenResponse = await client.SendAsync(tokenRequest);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    TempData["Loi"] = "Không thể lấy access token từ Google.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
                var accessToken = tokenJson.GetProperty("access_token").GetString();

                // 2. Lấy thông tin tài khoản từ Google UserInfo API
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var userInfoResponse = await client.SendAsync(userInfoRequest);
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    TempData["Loi"] = "Không thể lấy thông tin người dùng từ Google.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var userJson = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();
                var email = userJson.GetProperty("email").GetString();
                var name = userJson.TryGetProperty("name", out var n) ? n.GetString() : email;
                var id = userJson.GetProperty("id").GetString();

                var kq = await _taiKhoanService.XuLyDangNhapSocialAsync("Google", email!, name!, id);
                if (!kq.ThanhCong)
                {
                    TempData["Loi"] = kq.Loi;
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var tk = kq.DuLieu!;
                HttpContext.Session.SetThongTinDangNhap(tk.MaTaiKhoan, tk.HoTen, tk.VaiTro);
                TempData["ThanhCong"] = $"Xin chào {tk.HoTen}! Bạn đã đăng nhập thành công qua Google.";
                return ChuyenHuongSauDangNhap(tk, returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Loi"] = "Có lỗi xảy ra trong quá trình xác thực Google: " + ex.Message;
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        // ===================== ĐĂNG NHẬP ZALO =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult LoginZalo(string? returnUrl = null)
        {
            var appId = _configuration["Authentication:Zalo:AppId"];
            var appSecret = _configuration["Authentication:Zalo:AppSecret"];

            // Nếu chưa cấu hình Zalo OAuth trong appsettings.json, chuyển sang trang chọn tài khoản Zalo giả lập (Demo / Dev Mode)
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                return RedirectToAction(nameof(SocialDemo), new { provider = "Zalo", returnUrl });
            }

            var redirectUri = Url.Action(nameof(ZaloCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);
            var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl ?? "/"));
            var zaloAuthUrl = $"https://oauth.zaloapp.com/v4/permission?" +
                $"app_id={Uri.EscapeDataString(appId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? string.Empty)}" +
                $"&state={Uri.EscapeDataString(state)}";

            return Redirect(zaloAuthUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ZaloCallback(string? code, string? state, string? error)
        {
            string? returnUrl = null;
            if (!string.IsNullOrEmpty(state))
            {
                try
                {
                    returnUrl = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state));
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                TempData["Loi"] = "Đăng nhập Zalo thất bại hoặc bị hủy.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var appId = _configuration["Authentication:Zalo:AppId"];
            var appSecret = _configuration["Authentication:Zalo:AppSecret"];
            var redirectUri = Url.Action(nameof(ZaloCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);

            try
            {
                var client = _httpClientFactory.CreateClient();
                // 1. Đổi authorization code lấy Access Token từ Zalo
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth.zaloapp.com/v4/access_token");
                tokenRequest.Headers.Add("secret_key", appSecret);
                tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "app_id", appId! },
                    { "grant_type", "authorization_code" },
                    { "code", code }
                });

                var tokenResponse = await client.SendAsync(tokenRequest);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    TempData["Loi"] = "Không thể lấy access token từ Zalo.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (!tokenJson.TryGetProperty("access_token", out var tokenProp))
                {
                    TempData["Loi"] = "Phản hồi token từ Zalo không hợp lệ.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }
                var accessToken = tokenProp.GetString();

                // 2. Lấy thông tin người dùng từ Zalo Open API
                var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://graph.zalo.me/v2.0/me?fields=id,name,picture");
                profileRequest.Headers.Add("access_token", accessToken);

                var profileResponse = await client.SendAsync(profileRequest);
                if (!profileResponse.IsSuccessStatusCode)
                {
                    TempData["Loi"] = "Không thể lấy thông tin người dùng từ Zalo.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var userJson = await profileResponse.Content.ReadFromJsonAsync<JsonElement>();
                var id = userJson.GetProperty("id").GetString();
                var name = userJson.TryGetProperty("name", out var n) ? n.GetString() : $"Người dùng Zalo {id}";
                var email = $"{id}@zalo.me";

                var kq = await _taiKhoanService.XuLyDangNhapSocialAsync("Zalo", email, name!, id);
                if (!kq.ThanhCong)
                {
                    TempData["Loi"] = kq.Loi;
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }

                var tk = kq.DuLieu!;
                HttpContext.Session.SetThongTinDangNhap(tk.MaTaiKhoan, tk.HoTen, tk.VaiTro);
                TempData["ThanhCong"] = $"Xin chào {tk.HoTen}! Bạn đã đăng nhập thành công qua Zalo.";
                return ChuyenHuongSauDangNhap(tk, returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Loi"] = "Có lỗi xảy ra trong quá trình xác thực Zalo: " + ex.Message;
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        // ===================== DEMO SOCIAL LOGIN =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SocialDemo(string provider = "Google", string? returnUrl = null, bool isRegister = false)
        {
            var isConfigured = provider.Equals("Google", StringComparison.OrdinalIgnoreCase)
                ? !string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"])
                : !string.IsNullOrWhiteSpace(_configuration["Authentication:Zalo:AppId"]);

            var vm = new SocialLoginDemoViewModel
            {
                Provider = provider,
                ReturnUrl = returnUrl,
                IsRegister = isRegister,
                IsConfigured = isConfigured
            };
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SocialDemoLogin(SocialLoginDemoViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Email và Họ tên.");
                return View(nameof(SocialDemo), model);
            }

            var kq = await _taiKhoanService.XuLyDangNhapSocialAsync(
                model.Provider,
                model.Email,
                model.HoTen,
                soDienThoai: model.SoDienThoai,
                diaChi: model.DiaChi);

            if (!kq.ThanhCong)
            {
                ModelState.AddModelError("", kq.Loi ?? "Thao tác thất bại");
                return View(nameof(SocialDemo), model);
            }

            var tk = kq.DuLieu!;
            HttpContext.Session.SetThongTinDangNhap(tk.MaTaiKhoan, tk.HoTen, tk.VaiTro);

            if (model.IsRegister)
            {
                TempData["ThanhCong"] = $"Chúc mừng {tk.HoTen}! Bạn đã đăng ký thành viên thành công qua {model.Provider}. Ưu đãi thành viên đã được kích hoạt!";
            }
            else
            {
                TempData["ThanhCong"] = $"Đăng nhập thành công với tài khoản {model.Provider} ({tk.HoTen})!";
            }

            return ChuyenHuongSauDangNhap(tk, model.ReturnUrl);
        }

        // ===================== ĐĂNG KÝ QUA GMAIL (GOOGLE) =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterGoogle(string? returnUrl = null)
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                return RedirectToAction(nameof(SocialDemo), new { provider = "Google", returnUrl, isRegister = true });
            }

            var redirectUri = Url.Action(nameof(GoogleCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);
            var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl ?? "/"));
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? string.Empty)}" +
                $"&response_type=code" +
                $"&scope=openid%20profile%20email" +
                $"&state={Uri.EscapeDataString(state)}" +
                $"&prompt=select_account";

            return Redirect(googleAuthUrl);
        }

        // ===================== ĐĂNG KÝ QUA ZALO =====================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterZalo(string? returnUrl = null)
        {
            var appId = _configuration["Authentication:Zalo:AppId"];
            var appSecret = _configuration["Authentication:Zalo:AppSecret"];

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                return RedirectToAction(nameof(SocialDemo), new { provider = "Zalo", returnUrl, isRegister = true });
            }

            var redirectUri = Url.Action(nameof(ZaloCallback), "TaiKhoan", new { area = "User" }, Request.Scheme);
            var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(returnUrl ?? "/"));
            var zaloAuthUrl = $"https://oauth.zaloapp.com/v4/permission?" +
                $"app_id={Uri.EscapeDataString(appId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? string.Empty)}" +
                $"&state={Uri.EscapeDataString(state)}";

            return Redirect(zaloAuthUrl);
        }

        // ===================== ĐĂNG XUẤT =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Đăng xuất phải xóa Session
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        // ===================== TỪ CHỐI TRUY CẬP =====================
        // PhanQuyenAttribute chuyển hướng về đây khi sai vai trò
        [AllowAnonymous]
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // ===================== HELPER =====================
        private IActionResult ChuyenHuongSauDangNhap(TaiKhoan tk, string? returnUrl)
        {
            if (tk.VaiTro is VaiTroConst.Admin or VaiTroConst.NhanVien)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                && !returnUrl.Contains("Login", StringComparison.OrdinalIgnoreCase)
                && !returnUrl.Contains("Register", StringComparison.OrdinalIgnoreCase)
                && !returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        private IActionResult ChuyenHuongTheoVaiTro(string? vaiTro)
        {
            return vaiTro is VaiTroConst.Admin or VaiTroConst.NhanVien
                ? RedirectToAction("Index", "Home", new { area = "Admin" })
                : RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}
