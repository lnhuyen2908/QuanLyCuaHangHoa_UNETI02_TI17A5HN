using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.ViewModels;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Services
{
    public static class DonDatHoaInvoicePdf
    {
        // Văn hóa hiển thị tiền Việt Nam.
        private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

        // Định dạng tiền tệ.
        private static string Tien(decimal value) => value.ToString("N0", ViCulture) + " đ";

        // Tạo ô tiêu đề bảng.
        private static void HeaderCell(IContainer container, string value)
        {
            container
                .Background(Colors.Green.Darken2)
                .Padding(6)
                .Text(value)
                .Bold()
                .FontColor(Colors.White)
                .FontSize(9);
        }

        // Tạo ô dữ liệu bảng.
        private static void BodyCell(IContainer container, string value)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .Text(value)
                .FontSize(9);
        }

        // Tạo một dòng thông tin tổng tiền.
        private static void TotalLine(IContainer container, string label, string value)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(label);
                row.AutoItem().Text(value).Bold();
            });
        }

        // Tạo file PDF hóa đơn từ dữ liệu đơn hàng.
        public static byte[] TaoPdf(HoaDonPreviewViewModel hoaDon)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    // Cấu hình trang A4.
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(10));

                    // =========================
                    // PHẦN ĐẦU HÓA ĐƠN
                    // =========================
                    page.Header().Column(column =>
                    {
                        column.Item()
                            .AlignCenter()
                            .Text("HÓA ĐƠN ĐẶT HOA")
                            .Bold()
                            .FontSize(21)
                            .FontColor(Colors.Green.Darken2);

                        column.Item()
                            .PaddingTop(5)
                            .AlignCenter()
                            .Text($"Mã đơn: #ORD-{hoaDon.MaDon:D4}")
                            .Bold()
                            .FontSize(12);

                        column.Item()
                            .PaddingTop(3)
                            .AlignCenter()
                            .Text("Cảm ơn quý khách đã tin tưởng và sử dụng dịch vụ!");
                    });

                    // =========================
                    // NỘI DUNG HÓA ĐƠN
                    // =========================
                    page.Content()
                        .PaddingTop(20)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            // -------------------------
                            // 1. THÔNG TIN ĐƠN HÀNG & NGƯỜI NHẬN (2 CỘT)
                            // -------------------------
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(info =>
                                {
                                    info.Spacing(8);

                                    info.Item().Row(row =>
                                    {
                                        // Cột trái: Thông tin đơn hàng
                                        row.RelativeItem().Column(left =>
                                        {
                                            left.Spacing(5);
                                            left.Item().Text("THÔNG TIN ĐƠN HÀNG").Bold().FontSize(11);
                                            left.Item().Text($"Ngày đặt: {hoaDon.NgayDat:dd/MM/yyyy HH:mm}");
                                            left.Item().Text($"Khách hàng: {hoaDon.TenKhachHang}");
                                            left.Item().Text($"Hình thức nhận: {hoaDon.HinhThucNhan}");
                                        });

                                        // Khoảng cách giữa 2 cột
                                        row.ConstantItem(20);

                                        // Cột phải: Thông tin người nhận
                                        row.RelativeItem().Column(right =>
                                        {
                                            right.Spacing(5);
                                            right.Item().Text("THÔNG TIN NGƯỜI NHẬN").Bold().FontSize(11);
                                            right.Item().Text($"Người nhận: {hoaDon.NguoiNhan}");
                                            right.Item().Text($"Số điện thoại: {hoaDon.SoDienThoaiNguoiNhan}");
                                            right.Item().Text($"Địa chỉ: {hoaDon.DiaChiGiao}");
                                            right.Item().Text($"Thời gian nhận: {hoaDon.ThoiGianNhanGiao:dd/MM/yyyy HH:mm}");
                                        });
                                    });

                                    // Lời nhắn (nằm dưới 2 cột, chiếm toàn bộ chiều ngang)
                                    if (!string.IsNullOrWhiteSpace(hoaDon.LoiNhan))
                                    {
                                        info.Item().Text($"Lời nhắn: {hoaDon.LoiNhan}");
                                    }
                                });

                            // -------------------------
                            // 2. CHI TIẾT SẢN PHẨM
                            // -------------------------
                            column.Item()
                                .Text("CHI TIẾT SẢN PHẨM")
                                .Bold()
                                .FontSize(12);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(28);
                                    columns.RelativeColumn(4);
                                    columns.ConstantColumn(45);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Tiêu đề bảng.
                                table.Header(header =>
                                {
                                    HeaderCell(header.Cell(), "STT");
                                    HeaderCell(header.Cell(), "Sản phẩm");
                                    HeaderCell(header.Cell(), "SL");
                                    HeaderCell(header.Cell(), "Đơn giá");
                                    HeaderCell(header.Cell(), "Thành tiền");
                                });

                                // Dữ liệu sản phẩm.
                                for (int i = 0; i < hoaDon.ChiTiet.Count; i++)
                                {
                                    var item = hoaDon.ChiTiet[i];

                                    BodyCell(table.Cell(), (i + 1).ToString());
                                    BodyCell(table.Cell(), item.TenSanPham);
                                    BodyCell(table.Cell(), item.SoLuong.ToString());
                                    BodyCell(table.Cell(), Tien(item.DonGia));
                                    BodyCell(table.Cell(), Tien(item.ThanhTien));

                                    // Yêu cầu riêng của sản phẩm nếu có.
                                    if (!string.IsNullOrWhiteSpace(item.YeuCauRieng))
                                    {
                                        table.Cell()
                                            .ColumnSpan(5)
                                            .Padding(4)
                                            .Background(Colors.Grey.Lighten4)
                                            .Text($"Yêu cầu riêng: {item.YeuCauRieng}")
                                            .FontSize(9)
                                            .Italic();
                                    }
                                }
                            });

                            // -------------------------
                            // 3. TỔNG GIÁ TRỊ ĐƠN HÀNG
                            // -------------------------
                            column.Item()
                                .AlignRight()
                                .Width(260)
                                .Column(totals =>
                                {
                                    totals.Spacing(5);

                                    TotalLine(totals.Item(), "Phí giao hàng:", Tien(hoaDon.PhiGiaoHang));
                                    TotalLine(totals.Item(), "Phụ phí:", Tien(hoaDon.PhuPhi));

                                    totals.Item()
                                        .BorderTop(1)
                                        .BorderColor(Colors.Grey.Medium)
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("TỔNG GIÁ TRỊ")
                                                .Bold()
                                                .FontSize(11);

                                            row.AutoItem()
                                                .Text(Tien(hoaDon.TongTien))
                                                .Bold()
                                                .FontSize(11)
                                                .FontColor(Colors.Green.Darken2);
                                        });
                                });

                            // -------------------------
                            // 4. LỊCH SỬ THANH TOÁN
                            // -------------------------
                            column.Item()
                                .Text("LỊCH SỬ THANH TOÁN")
                                .Bold()
                                .FontSize(12);

                            if (hoaDon.ThanhToans.Count == 0)
                            {
                                column.Item().Text("Chưa ghi nhận khoản thanh toán nào.");
                            }
                            else
                            {
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    // Tiêu đề bảng thanh toán.
                                    table.Header(header =>
                                    {
                                        HeaderCell(header.Cell(), "Thời gian");
                                        HeaderCell(header.Cell(), "Phương thức");
                                        HeaderCell(header.Cell(), "Số tiền");
                                    });

                                    // Các lần thanh toán.
                                    foreach (var payment in hoaDon.ThanhToans)
                                    {
                                        BodyCell(table.Cell(), payment.NgayThanhToan.ToString("dd/MM/yyyy HH:mm"));
                                        BodyCell(table.Cell(), payment.PhuongThuc);
                                        BodyCell(table.Cell(), Tien(payment.SoTien));
                                    }
                                });
                            }

                            // -------------------------
                            // 5. TỔNG KẾT THANH TOÁN
                            // -------------------------
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(summary =>
                                {
                                    summary.Spacing(6);

                                    TotalLine(summary.Item(), "Tổng giá trị đơn hàng:", Tien(hoaDon.TongTien));
                                    TotalLine(summary.Item(), "Tổng đã thanh toán:", Tien(hoaDon.DaThanhToan));

                                    summary.Item()
                                        .BorderTop(1)
                                        .BorderColor(Colors.Grey.Medium)
                                        .PaddingTop(6)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("CÒN PHẢI THANH TOÁN")
                                                .Bold()
                                                .FontSize(12);

                                            row.AutoItem()
                                                .Text(Tien(hoaDon.ConLai))
                                                .Bold()
                                                .FontSize(12)
                                                .FontColor(Colors.Red.Darken2);
                                        });
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Hóa đơn được xuất từ hệ thống quản lý cửa hàng hoa.");
                        });
                });
            }).GeneratePdf();
        }
    }
}