// Họ và tên: Lã Ngọc Huyền
// Mã sinh viên: 23103100271
// Nội dung thực hiện: Dữ liệu mẫu cho toàn bộ 13 entity (tài khoản, danh mục, hoa, mẫu, khách hàng, đơn, giao nhận, thanh toán, đánh giá).

using Microsoft.EntityFrameworkCore;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models;
using QuanLyCuaHangHoa_UNETI02_TI17A5HN.Models.Entities;

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Data
{
    public static class SeedData
    {
        private const string MatKhauMacDinh = "123456";
        private const string DiaChiCuaHang = "Nhận tại cửa hàng - 218 Lĩnh Nam, Hoàng Mai, Hà Nội";

        // ===== Dữ liệu thô =====
        private static readonly string[] TenKhach =
        {
            "Nguyễn Thu Thảo","Trần Minh Hoàng","Lê Phương Uyên","Phạm Hoàng Long","Đỗ Minh Anh","Hoàng Minh Trí",
            "Vũ Thanh Hà","Bùi Quốc Vũ","Đặng Ngọc Lan","Ngô Thị Mai","Dương Văn Hiếu","Lý Gia Hân",
            "Phan Thị Hương","Võ Anh Tuấn","Trịnh Khánh Linh","Đinh Công Danh","Mai Phương Thảo","Tạ Quang Huy",
            "Hồ Thu Trang","Lương Bảo Châu","Châu Minh Khôi","Kiều Diễm My","Nguyễn Hải Yến","Lê Đức Thịnh"
        };

        private static readonly string[] TenNguoiNhan =
            { "Nguyễn Mai Phương","Lê Thị Hạnh","Trần Quốc Bảo","Phạm Thu Hiền","Vũ Ngọc Ánh","Hoàng Thị Lan","Đỗ Minh Quân","Bùi Thanh Tâm" };

        private static readonly string[] DiaChiKhach =
        {
            "12 Ngõ 218 Lĩnh Nam, Hoàng Mai, Hà Nội","45 Tràng Tiền, Hoàn Kiếm, Hà Nội","88 Cầu Giấy, Cầu Giấy, Hà Nội",
            "23 Nguyễn Trãi, Thanh Xuân, Hà Nội","56 Kim Mã, Ba Đình, Hà Nội","9 Trần Duy Hưng, Cầu Giấy, Hà Nội",
            "101 Giải Phóng, Hai Bà Trưng, Hà Nội","34 Xuân Thủy, Cầu Giấy, Hà Nội"
        };

        private static readonly string[] DiaChiGiao =
        {
            "Tầng 8, Tòa Landmark, Phạm Hùng, Nam Từ Liêm, Hà Nội","15 Hàng Bài, Hoàn Kiếm, Hà Nội",
            "Số 7 Ngõ 120 Trường Chinh, Đống Đa, Hà Nội","Căn 1205, Vinhomes Times City, Hai Bà Trưng, Hà Nội",
            "22 Thụy Khuê, Tây Hồ, Hà Nội","Công ty ABC, 99 Láng Hạ, Đống Đa, Hà Nội",
            "66 Nguyễn Chí Thanh, Đống Đa, Hà Nội","Sảnh B, Royal City, Thanh Xuân, Hà Nội"
        };

        private static readonly string[] LoiNhan =
        {
            "Chúc mừng sinh nhật em, mãi rạng rỡ như những đóa hoa!","Cảm ơn mẹ vì tất cả yêu thương.",
            "Chúc công ty khai trương hồng phát, vạn sự hanh thông!","Kỷ niệm một năm hạnh phúc, anh yêu em.",
            "Chúc mừng tốt nghiệp, chúc con bước vào đời thật vững vàng!","Gửi lời tri ân chân thành đến thầy cô.",
            "Chúc mừng thăng chức, tiền đồ rộng mở!","Mãi bên nhau em nhé."
        };

        private static readonly string[] YeuCauRieng =
        {
            "Kiểu: Bó hoa cầm tay | Tông: hồng pastel | Hoa ưu tiên: hồng Ohara, tulip trắng | Thiệp: Chúc mừng lễ đính hôn | Ngân sách: 1.500.000 - 2.000.000đ",
            "Kiểu: Giỏ hoa mây | Tông: vàng ấm & cam | Hoa ưu tiên: hướng dương, hồng cam | Thiệp: Chúc mừng tân gia | Ngân sách: 2.000.000 - 2.500.000đ",
            "Kiểu: Hộp hoa tròn | Tông: trắng kem & bordeaux | Hoa ưu tiên: lan hồ điệp, hồng đỏ | Thiệp: Mãi yêu em | Ngân sách: 2.500.000 - 3.000.000đ",
            "Kiểu: Kệ hoa khai trương | Tông: đỏ & vàng kim | Hoa ưu tiên: hồng Ecuador, lan vàng | Thiệp: Mã đáo thành công | Ngân sách: 3.000.000 - 4.000.000đ",
            "Kiểu: Bó hoa cầm tay | Tông: tím lãng mạn | Hoa ưu tiên: hồng tím, baby | Thiệp: Happy Anniversary | Ngân sách: 1.500.000 - 2.000.000đ"
        };

        private static readonly string[] NoiDungDanhGia =
        {
            "Hoa tươi, giao đúng giờ, gói rất đẹp. Rất hài lòng!","Bó hoa đúng như hình, người nhận rất thích.",
            "Chất lượng tốt, nhân viên tư vấn nhiệt tình.","Hoa đẹp nhưng giao hơi trễ một chút.",
            "Thiết kế riêng đúng ý, florist rất tâm lý.","Sẽ tiếp tục ủng hộ shop dài dài.",
            "Hoa giữ tươi được hơn 5 ngày, tuyệt vời.","Đóng gói cẩn thận, hoa không bị dập."
        };

        private static readonly int[] SoSao = { 5, 5, 4, 5, 3, 4, 5, 5, 4, 5, 5, 4, 5, 5, 4 };

        private static readonly string[] Shipper = { "Nguyễn Hữu Đạt", "Trần Văn Nam", "Lê Quang Minh" };

        private static readonly string[] LyDoHuy =
        {
            "Khách thay đổi kế hoạch","Khách đặt nhầm mẫu","Không liên hệ được người nhận","Hết nguyên liệu theo yêu cầu thiết kế"
        };

        // ===== Điểm vào =====
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await SeedAsync(db);
        }

        public static async Task SeedAsync(AppDbContext db)
        {
            // Chỉ seed khi DB còn trống
            if (await db.TaiKhoans.AnyAsync()) return;

            // ================= 1. TÀI KHOẢN =================
            var taiKhoans = new List<TaiKhoan>
            {
                new() { TenDangNhap = "admin", MatKhau = MatKhauMacDinh, HoTen = "Quản trị viên", Email = "admin@nhom2atelier.vn", VaiTro = VaiTroConst.Admin },
                new() { TenDangNhap = "nhanvien01", MatKhau = MatKhauMacDinh, HoTen = "Lê Thị Mai", Email = "nhanvien01@nhom2atelier.vn", VaiTro = VaiTroConst.NhanVien },
                new() { TenDangNhap = "nhanvien02", MatKhau = MatKhauMacDinh, HoTen = "Phạm Kim Ngân", Email = "nhanvien02@nhom2atelier.vn", VaiTro = VaiTroConst.NhanVien }
            };
            for (int i = 0; i < TenKhach.Length; i++)
            {
                taiKhoans.Add(new TaiKhoan
                {
                    TenDangNhap = $"khachhang{i + 1:D2}",
                    MatKhau = MatKhauMacDinh,
                    HoTen = TenKhach[i],
                    Email = $"khachhang{i + 1:D2}@gmail.com",
                    VaiTro = VaiTroConst.KhachHang
                });
            }
            // Tài khoản bị khóa để test đăng nhập
            taiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = "khoa01", MatKhau = MatKhauMacDinh, HoTen = "Tài khoản bị khóa",
                Email = "khoa01@gmail.com", VaiTro = VaiTroConst.KhachHang, TrangThai = false
            });
            db.TaiKhoans.AddRange(taiKhoans);

            // ================= 2. LOẠI HOA & DỊP SỬ DỤNG =================
            var loaiHoas = new List<LoaiHoa>
            {
                new() { TenLoaiHoa = "Hoa hồng", MoTa = "Hồng nhập khẩu Pháp, Ecuador và hồng cổ David Austin" },
                new() { TenLoaiHoa = "Hoa tulip", MoTa = "Tulip Hà Lan nhiều sắc màu" },
                new() { TenLoaiHoa = "Cẩm tú cầu", MoTa = "Cẩm tú cầu Hydrangea tán lớn" },
                new() { TenLoaiHoa = "Hoa baby", MoTa = "Baby trắng và baby nhuộm màu" },
                new() { TenLoaiHoa = "Mẫu đơn", MoTa = "Mẫu đơn Peony nhập khẩu theo mùa" },
                new() { TenLoaiHoa = "Hoa hướng dương", MoTa = "Hướng dương Đà Lạt rực rỡ" },
                new() { TenLoaiHoa = "Lan hồ điệp", MoTa = "Phong lan Phalaenopsis sang trọng" },
                new() { TenLoaiHoa = "Hoa ly", MoTa = "Ly kép thơm không phấn" }
            };
            var dipSuDungs = new List<DipSuDung>
            {
                new() { TenDip = "Sinh nhật", MoTa = "Hoa chúc mừng sinh nhật người thân, bạn bè, đối tác" },
                new() { TenDip = "Kỷ niệm & Tình yêu", MoTa = "Valentine, kỷ niệm ngày cưới, tỏ tình" },
                new() { TenDip = "Khai trương", MoTa = "Kệ hoa, lẵng hoa chúc mừng khai trương" },
                new() { TenDip = "Chúc mừng", MoTa = "Thăng chức, tân gia, đạt thành tích" },
                new() { TenDip = "Tốt nghiệp", MoTa = "Hoa chúc mừng lễ tốt nghiệp" },
                new() { TenDip = "Cảm ơn & Tri ân", MoTa = "Ngày 20/10, 20/11, 8/3, tri ân thầy cô, mẹ" },
                new() { TenDip = "Chia buồn", MoTa = "Hoa tưởng nhớ, chia buồn" },
                new() { TenDip = "Hoa Tết", MoTa = "Dịp Tết - tạm ngừng hoạt động", TrangThai = false }
            };
            db.LoaiHoas.AddRange(loaiHoas);
            db.DipSuDungs.AddRange(dipSuDungs);
            await db.SaveChangesAsync();

            var admin = taiKhoans[0];
            var nvIds = new[] { taiKhoans[1].MaTaiKhoan, taiKhoans[2].MaTaiKhoan };

            // ================= 3. HOA TƯƠI =================
            var hoaData = new (string Ten, int Loai, string Mau, string Dvt, decimal Gia, bool Active)[]
            {
                ("Hồng Ohara Pháp",0,"Hồng phấn","Cành",65000,true),
                ("Hồng Ecuador đỏ",0,"Đỏ nhung","Cành",45000,true),
                ("Hồng Juliet David Austin",0,"Cam đào","Cành",90000,true),
                ("Hồng trắng Avalanche",0,"Trắng","Cành",40000,true),
                ("Hồng Tiara tím",0,"Tím","Cành",55000,true),
                ("Tulip trắng Hà Lan",1,"Trắng","Cành",50000,true),
                ("Tulip hồng",1,"Hồng","Cành",48000,true),
                ("Tulip vàng",1,"Vàng","Cành",48000,true),
                ("Cẩm tú cầu xanh",2,"Xanh lam","Cành",85000,true),
                ("Cẩm tú cầu trắng",2,"Trắng","Cành",80000,true),
                ("Cẩm tú cầu hồng pastel",2,"Hồng pastel","Cành",82000,true),
                ("Baby trắng",3,"Trắng","Bó",35000,true),
                ("Baby nhuộm hồng",3,"Hồng","Bó",40000,true),
                ("Mẫu đơn hồng Sarah Bernhardt",4,"Hồng","Cành",120000,true),
                ("Mẫu đơn trắng",4,"Trắng","Cành",130000,true),
                ("Hướng dương Đà Lạt",5,"Vàng","Cành",25000,true),
                ("Hướng dương mini",5,"Vàng cam","Cành",20000,true),
                ("Lan hồ điệp trắng",6,"Trắng","Cành",150000,true),
                ("Lan hồ điệp vàng",6,"Vàng","Cành",160000,true),
                ("Lan hồ điệp tím",6,"Tím","Cành",155000,true),
                ("Ly kép thơm trắng",7,"Trắng","Cành",70000,true),
                ("Ly kép hồng",7,"Hồng","Cành",68000,true),
                ("Hồng Spirit cam",0,"Cam","Cành",50000,true),
                ("Hồng kem Leonora",0,"Kem","Cành",75000,false)   // hết mùa
            };
            var hoaTuois = hoaData.Select(h => new HoaTuoi
            {
                TenHoa = h.Ten, MaLoaiHoa = loaiHoas[h.Loai].MaLoaiHoa, MauSac = h.Mau, DonViTinh = h.Dvt,
                DonGiaThamKhao = h.Gia, MoTa = $"{h.Ten} - nhập tuyển chọn trong ngày", TrangThai = h.Active
            }).ToList();

            // ================= 4. MẪU SẢN PHẨM =================
            var mauData = new (string Ten, int Dip, string Kieu, decimal Gia, bool Active)[]
            {
                ("Rosée Éternelle",1,KieuSanPhamConst.BoHoa,1450000,true),
                ("Sunset Romance",1,KieuSanPhamConst.BoHoa,1750000,true),
                ("Pure Innocence",1,KieuSanPhamConst.BoHoa,1250000,true),
                ("Mây Hồng Tình Yêu",0,KieuSanPhamConst.BoHoa,950000,true),
                ("Nụ Cười Tháng Tư",3,KieuSanPhamConst.BoHoa,580000,true),
                ("Juliet Serenade",0,KieuSanPhamConst.BoHoa,1890000,true),
                ("Amour Éternel",1,KieuSanPhamConst.BoHoa,1650000,true),
                ("Lumière Blanche",3,KieuSanPhamConst.BoHoa,1680000,true),
                ("Vintage Romance",0,KieuSanPhamConst.BoHoa,1350000,false),   // ngừng bán
                ("Mon Amour",1,KieuSanPhamConst.BoHoa,1250000,true),
                ("Hoàng Kim Tốt Nghiệp",4,KieuSanPhamConst.BoHoa,780000,true),
                ("Giỏ Hoa Bình Minh",0,KieuSanPhamConst.GioHoa,1850000,true),
                ("Giấc Mộng Provence",5,KieuSanPhamConst.GioHoa,1650000,true),
                ("Pastel Serenade",3,KieuSanPhamConst.GioHoa,950000,true),
                ("Thanh Âm Mùa Hạ",0,KieuSanPhamConst.GioHoa,1550000,true),
                ("Giỏ Mây Pastel Bloom",0,KieuSanPhamConst.GioHoa,2200000,true),
                ("Giỏ Tri Ân Sắc Xuân",5,KieuSanPhamConst.GioHoa,1100000,true),
                ("Velvet Noir",1,KieuSanPhamConst.HopHoa,2400000,true),
                ("Le Jardin Secret",1,KieuSanPhamConst.HopHoa,2850000,true),
                ("Serenade Hộp Tròn",3,KieuSanPhamConst.HopHoa,2100000,true),
                ("Hộp Hoàng Gia Serenata",0,KieuSanPhamConst.HopHoa,950000,true),
                ("Hộp Sweet Love",1,KieuSanPhamConst.HopHoa,2200000,true),
                ("Khởi Sắc Thịnh Vượng",2,KieuSanPhamConst.KeHoa,3150000,true),
                ("Đại Cát Đại Lợi",2,KieuSanPhamConst.KeHoa,3800000,true),
                ("Kệ Grand Opening",2,KieuSanPhamConst.KeHoa,4200000,true),
                ("Kệ Chúc Mừng Thăng Tiến",3,KieuSanPhamConst.KeHoa,3500000,false) // ngừng bán
            };
            var mauSanPhams = mauData.Select((m, idx) => new MauSanPham
            {
                TenMau = m.Ten, MaDip = dipSuDungs[m.Dip].MaDip, KieuSanPham = m.Kieu, GiaCoBan = m.Gia,
                HinhAnh = $"/images/mau/mau{idx + 1:D2}.jpg",
                MoTa = $"{m.Ten} - {m.Kieu.ToLower()} thiết kế bởi Master Florist.",
                TrangThai = m.Active, NgayTao = DateTime.Today.AddDays(-200 + idx * 5)
            }).ToList();

            // ================= 5. KHÁCH HÀNG =================
            var khachHangs = new List<KhachHang>();
            for (int i = 0; i < TenKhach.Length; i++)
            {
                var acc = taiKhoans[3 + i];
                khachHangs.Add(new KhachHang
                {
                    MaTaiKhoan = acc.MaTaiKhoan, HoTen = acc.HoTen, SoDienThoai = Phone(i + 1),
                    Email = acc.Email, DiaChi = DiaChiKhach[i % DiaChiKhach.Length]
                });
            }

            db.HoaTuois.AddRange(hoaTuois);
            db.MauSanPhams.AddRange(mauSanPhams);
            db.KhachHangs.AddRange(khachHangs);
            await db.SaveChangesAsync();

            // ================= 6. CHI TIẾT MẪU (mỗi mẫu 3 loại hoa khác nhau) =================
            var hoaDangDung = hoaTuois.Where(h => h.TrangThai).ToList();
            int n = hoaDangDung.Count;
            string[] ghiChuHoa = { "Hoa chính", "Hoa phụ", "Hoa phối" };
            var chiTietMaus = new List<ChiTietMau>();
            for (int k = 0; k < mauSanPhams.Count; k++)
            {
                int[] viTri = { (k * 2) % n, (k * 2 + 5) % n, (k * 2 + 11) % n };
                int[] soLuong = { 8 + (k % 5) * 2, 4 + k % 3, 2 + k % 2 };
                for (int j = 0; j < 3; j++)
                {
                    chiTietMaus.Add(new ChiTietMau
                    {
                        MaMau = mauSanPhams[k].MaMau, MaHoa = hoaDangDung[viTri[j]].MaHoa,
                        SoLuongTieuChuan = soLuong[j], GhiChu = ghiChuHoa[j]
                    });
                }
            }
            db.ChiTietMaus.AddRange(chiTietMaus);
            await db.SaveChangesAsync();

            // ================= 7. ĐƠN ĐẶT HOA + CHI TIẾT + LỊCH SỬ + GIAO NHẬN + THANH TOÁN + ĐÁNH GIÁ =================
            var mauDangBan = mauSanPhams.Where(m => m.TrangThai).ToList();

            // 45 đơn, xếp từ cũ -> mới
            var trangThais = new List<string>();
            void Them(string tt, int soLuong) { for (int x = 0; x < soLuong; x++) trangThais.Add(tt); }
            Them(TrangThaiDon.HoanThanh, 15);
            Them(TrangThaiDon.DaHuy, 4);
            Them(TrangThaiDon.DangGiao, 5);
            Them(TrangThaiDon.SanSang, 4);
            Them(TrangThaiDon.DangChuanBi, 6);
            Them(TrangThaiDon.DaXacNhan, 5);
            Them(TrangThaiDon.ChoXacNhan, 6);

            var today = DateTime.Today;
            var donHangs = new List<DonDatHoa>();

            for (int i = 0; i < trangThais.Count; i++)
            {
                var tt = trangThais[i];
                var kh = khachHangs[i % khachHangs.Count];

                int daysAgo = i < 15 ? 120 - i * 7 : i < 19 ? 20 - (i - 15) * 3 : 6 - (i - 19) / 5;
                var ngayDat = today.AddDays(-daysAgo).AddHours(8 + i % 10).AddMinutes((i * 13) % 60);

                bool giaoTanNoi = tt == TrangThaiDon.DangGiao || i % 4 != 0;
                bool daXong = tt == TrangThaiDon.HoanThanh || tt == TrangThaiDon.DaHuy;
                DateTime tgNhan = daXong
                    ? ngayDat.Date.AddDays(1).AddHours(9 + i % 8)
                    : (tt == TrangThaiDon.SanSang || tt == TrangThaiDon.DangGiao)
                        ? today.AddHours(10 + i % 8)
                        : today.AddDays(1 + i % 3).AddHours(9 + i % 8);

                // ----- Chi tiết đơn -----
                var chiTiets = new List<ChiTietDonHoa>();
                bool thietKeRieng = i % 6 == 3 || i == 42;
                if (thietKeRieng)
                {
                    decimal giaRieng = 1_500_000 + (i % 4) * 500_000;
                    chiTiets.Add(new ChiTietDonHoa
                    {
                        MaMau = null, SoLuong = 1, DonGia = giaRieng, ThanhTien = giaRieng,
                        YeuCauRieng = YeuCauRieng[i % YeuCauRieng.Length],
                        DaChotGia = tt != TrangThaiDon.ChoXacNhan && tt != TrangThaiDon.DaHuy
                    });
                }
                else
                {
                    var m1 = mauDangBan[(i * 3) % mauDangBan.Count];
                    int sl1 = i % 5 == 0 ? 2 : 1;
                    chiTiets.Add(new ChiTietDonHoa { MaMau = m1.MaMau, SoLuong = sl1, DonGia = m1.GiaCoBan, ThanhTien = sl1 * m1.GiaCoBan, DaChotGia = true });
                    if (i % 3 == 1)
                    {
                        var m2 = mauDangBan[(i * 5 + 2) % mauDangBan.Count];
                        if (m2.MaMau != m1.MaMau)
                            chiTiets.Add(new ChiTietDonHoa { MaMau = m2.MaMau, SoLuong = 1, DonGia = m2.GiaCoBan, ThanhTien = m2.GiaCoBan, DaChotGia = true });
                    }
                }

                // ----- Tính tiền -----
                decimal tongHang = chiTiets.Sum(c => c.ThanhTien);
                decimal phiGiao = giaoTanNoi ? (tongHang >= 1_500_000 ? 0 : 30_000) : 0;
                bool coPhuPhi = giaoTanNoi && i % 8 == 5;
                decimal phuPhi = coPhuPhi ? 50_000 : 0;

                var don = new DonDatHoa
                {
                    MaKhachHang = kh.MaKhachHang,
                    NgayDat = ngayDat,
                    HinhThucNhan = giaoTanNoi ? HinhThucNhanConst.GiaoTanNoi : HinhThucNhanConst.KhachDenNhan,
                    ThoiGianNhanGiao = tgNhan,
                    DiaChiGiao = giaoTanNoi ? DiaChiGiao[i % DiaChiGiao.Length] : null,
                    NguoiNhan = i % 2 == 0 ? kh.HoTen : TenNguoiNhan[i % TenNguoiNhan.Length],
                    SoDienThoaiNguoiNhan = Phone(100 + i),
                    LoiNhan = LoiNhan[i % LoiNhan.Length],
                    TrangThai = tt,
                    GhiChu = i % 5 == 0 ? "Gọi trước khi giao 15 phút" : null,
                    PhiGiaoHang = phiGiao,
                    PhuPhi = phuPhi,
                    LyDoPhuPhi = coPhuPhi ? "Phụ phí giao hỏa tốc trong 2 giờ" : null,
                    TongTien = tongHang + phiGiao + phuPhi,
                    LyDoHuy = tt == TrangThaiDon.DaHuy ? LyDoHuy[i - 15] : null,
                    ChiTietDonHoas = chiTiets
                };

                // ----- Lịch sử xử lý -----
                var flow = giaoTanNoi
                    ? new[] { TrangThaiDon.ChoXacNhan, TrangThaiDon.DaXacNhan, TrangThaiDon.DangChuanBi, TrangThaiDon.SanSang, TrangThaiDon.DangGiao, TrangThaiDon.HoanThanh }
                    : new[] { TrangThaiDon.ChoXacNhan, TrangThaiDon.DaXacNhan, TrangThaiDon.DangChuanBi, TrangThaiDon.SanSang, TrangThaiDon.HoanThanh };

                List<string> path;
                if (tt == TrangThaiDon.DaHuy)
                {
                    path = new List<string> { TrangThaiDon.ChoXacNhan };
                    if (i == 16) path.Add(TrangThaiDon.DaXacNhan);
                    path.Add(TrangThaiDon.DaHuy);
                }
                else
                {
                    path = flow.Take(Array.IndexOf(flow, tt) + 1).ToList();
                }

                for (int k = 0; k < path.Count; k++)
                {
                    int nguoi = k == 0 ? kh.MaTaiKhoan
                              : path[k] == TrangThaiDon.DaHuy ? (i == 15 ? kh.MaTaiKhoan : admin.MaTaiKhoan)
                              : nvIds[(i + k) % nvIds.Length];
                    don.LichSuXuLyDons.Add(new LichSuXuLyDon
                    {
                        TrangThaiCu = k == 0 ? null : path[k - 1],
                        TrangThaiMoi = path[k],
                        ThoiGian = ngayDat.AddHours(k * 3),
                        NguoiThucHien = nguoi,
                        GhiChu = k == 0 ? "Khách đặt hoa"
                               : path[k] == TrangThaiDon.DaHuy ? $"Hủy đơn: {don.LyDoHuy}"
                               : $"Chuyển sang '{path[k]}'"
                    });
                }

                // ----- Giao nhận -----
                if (tt != TrangThaiDon.ChoXacNhan && tt != TrangThaiDon.DaHuy)
                {
                    bool daPhanCong = tt == TrangThaiDon.SanSang || tt == TrangThaiDon.DangGiao || tt == TrangThaiDon.HoanThanh;
                    don.GiaoNhans.Add(new GiaoNhan
                    {
                        HinhThuc = don.HinhThucNhan,
                        ThoiGianDuKien = tgNhan,
                        ThoiGianThucTe = tt == TrangThaiDon.HoanThanh ? tgNhan.AddMinutes(-10 + (i % 5) * 7) : null,
                        DiaChi = giaoTanNoi ? don.DiaChiGiao : DiaChiCuaHang,
                        NguoiGiao = giaoTanNoi && daPhanCong ? Shipper[i % Shipper.Length] : null,
                        TrangThai = tt == TrangThaiDon.HoanThanh ? TrangThaiGiaoNhanConst.HoanThanh
                                  : tt == TrangThaiDon.DangGiao ? TrangThaiGiaoNhanConst.DangGiao
                                  : TrangThaiGiaoNhanConst.ChoXuLy,
                        GhiChu = giaoTanNoi ? "Giao bằng xe lạnh chuyên dụng" : "Khách đến nhận tại quầy"
                    });
                }

                // ----- Thanh toán (không bao giờ vượt tổng tiền) -----
                string[] pt = { PhuongThucThanhToanConst.TienMat, PhuongThucThanhToanConst.ChuyenKhoan,
                                PhuongThucThanhToanConst.ViDienTu, PhuongThucThanhToanConst.TheNganHang };
                decimal tong = don.TongTien;
                decimal coc = Math.Round(tong * 0.5m / 1000m) * 1000m;
                int nv = nvIds[i % nvIds.Length];

                switch (tt)
                {
                    case TrangThaiDon.HoanThanh:
                        if (i % 3 == 0) // thanh toán 2 lần: cọc + còn lại
                        {
                            don.ThanhToans.Add(TaoThanhToan(coc, ngayDat.AddHours(1), PhuongThucThanhToanConst.ChuyenKhoan, nv, "Đặt cọc 50%"));
                            don.ThanhToans.Add(TaoThanhToan(tong - coc, tgNhan, PhuongThucThanhToanConst.TienMat, nv, "Thanh toán phần còn lại khi nhận hoa"));
                        }
                        else
                        {
                            don.ThanhToans.Add(TaoThanhToan(tong, i % 2 == 0 ? ngayDat.AddHours(1) : tgNhan, pt[i % 4], nv, "Thanh toán đủ"));
                        }
                        break;
                    case TrangThaiDon.DaXacNhan:
                    case TrangThaiDon.DangChuanBi:
                        if (i % 2 == 0)
                            don.ThanhToans.Add(TaoThanhToan(coc, ngayDat.AddHours(2), PhuongThucThanhToanConst.ChuyenKhoan, nv, "Đặt cọc 50%"));
                        break;
                    case TrangThaiDon.SanSang:
                        if (i % 2 == 0)
                            don.ThanhToans.Add(TaoThanhToan(coc, ngayDat.AddHours(2), PhuongThucThanhToanConst.ViDienTu, nv, "Đặt cọc 50%"));
                        break;
                    case TrangThaiDon.DangGiao:
                        if (i % 3 != 0) // i % 3 == 0: COD, chưa thanh toán
                            don.ThanhToans.Add(TaoThanhToan(coc, ngayDat.AddHours(2), PhuongThucThanhToanConst.ChuyenKhoan, nv, "Đặt cọc 50%"));
                        break;
                }

                // ----- Đánh giá (chỉ đơn hoàn thành, bỏ qua một số đơn) -----
                if (tt == TrangThaiDon.HoanThanh && i % 4 != 3)
                {
                    don.DanhGia = new DanhGia
                    {
                        MaKhachHang = kh.MaKhachHang,
                        SoSao = SoSao[i % SoSao.Length],
                        NoiDung = NoiDungDanhGia[i % NoiDungDanhGia.Length],
                        NgayDanhGia = tgNhan.AddHours(20)
                    };
                }

                donHangs.Add(don);
            }

            db.DonDatHoas.AddRange(donHangs);
            await db.SaveChangesAsync();
        }

        // ===== Helpers nội bộ =====
        private static string Phone(int seed) => "09" + ((seed * 7919L + 13579) % 100000000).ToString("D8");

        private static ThanhToan TaoThanhToan(decimal soTien, DateTime ngay, string phuongThuc, int nguoiGhiNhan, string? ghiChu) => new()
        {
            SoTien = soTien, NgayThanhToan = ngay, PhuongThuc = phuongThuc, NguoiGhiNhan = nguoiGhiNhan, GhiChu = ghiChu
        };
    }
}