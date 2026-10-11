// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Filter kiểm tra đăng nhập (Session) và vai trò tại Controller, hỗ trợ [AllowAnonymous].

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Helpers
{
    // Tương đương [Authorize] của ASP.NET Core nhưng dựa trên Session:
    //   [PhanQuyen]                                            -> chỉ cần đã đăng nhập      (như [Authorize])
    //   [PhanQuyen(VaiTroConst.Admin)]                         -> chỉ Admin                  (như [Authorize(Roles="Admin")])
    //   [PhanQuyen(VaiTroConst.Admin, VaiTroConst.NhanVien)]   -> Admin HOẶC Nhân viên       (như [Authorize(Roles="Admin,NhanVien")])
    //   [AllowAnonymous] trên Action                           -> mở ngoại lệ công khai
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PhanQuyenAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _vaiTroChoPhep;

        public PhanQuyenAttribute(params string[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep ?? Array.Empty<string>();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Bước 0: Action có [AllowAnonymous] -> bỏ qua kiểm tra.
            // Nếu thiếu bước này, đặt [PhanQuyen] ở cấp Controller sẽ chặn luôn
            // trang Login và gây vòng lặp chuyển hướng vô tận.
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
                return;

            var http = context.HttpContext;
            var session = http.Session;
            var laAjax = LaYeuCauAjax(http.Request);

            // Bước 1: Chưa đăng nhập -> 401 -> về trang Login
            if (!session.DaDangNhap())
            {
                context.Result = laAjax
                    ? new StatusCodeResult(StatusCodes.Status401Unauthorized)
                    : new RedirectToActionResult("Login", "TaiKhoan",
                        new { area = "User", returnUrl = LayReturnUrl(http.Request) });
                return;
            }

            // Kiểm tra tài khoản có bị khóa trong CSDL hay không (phòng trường hợp bị khóa khi session còn hạn)
            var maTaiKhoan = session.GetMaTaiKhoan();
            if (maTaiKhoan.HasValue)
            {
                var taiKhoanService = http.RequestServices.GetRequiredService<Services.TaiKhoanService>();
                if (!await taiKhoanService.ConHoatDongAsync(maTaiKhoan.Value))
                {
                    session.Clear();
                    context.Result = laAjax
                        ? new StatusCodeResult(StatusCodes.Status401Unauthorized)
                        : new RedirectToActionResult("Login", "TaiKhoan",
                            new { area = "User", returnUrl = LayReturnUrl(http.Request) });
                    return;
                }
            }

            // Bước 2: Đã đăng nhập nhưng sai vai trò -> 403 -> trang từ chối truy cập
            if (_vaiTroChoPhep.Length > 0 && !session.CoVaiTro(_vaiTroChoPhep))
            {
                context.Result = laAjax
                    ? new StatusCodeResult(StatusCodes.Status403Forbidden)
                    : new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", new { area = "User" });
            }

            // Hợp lệ (200): không gán context.Result -> request chạy tiếp vào Action
        }

        // Chỉ quay lại bằng returnUrl với GET. Với POST, quay lại sẽ thành GET
        // vào một Action chỉ nhận POST và gây lỗi 405.
        private static string? LayReturnUrl(HttpRequest request)
            => HttpMethods.IsGet(request.Method) ? request.Path + request.QueryString : null;

        private static bool LaYeuCauAjax(HttpRequest request)
            => request.Headers.XRequestedWith == "XMLHttpRequest"
               || request.Headers.Accept.ToString().Contains("application/json");
    }
}