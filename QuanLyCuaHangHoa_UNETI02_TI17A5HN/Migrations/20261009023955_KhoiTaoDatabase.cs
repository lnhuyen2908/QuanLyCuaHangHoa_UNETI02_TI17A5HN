using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DipSuDungs",
                columns: table => new
                {
                    MaDip = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDip = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DipSuDungs", x => x.MaDip);
                });

            migrationBuilder.CreateTable(
                name: "LoaiHoas",
                columns: table => new
                {
                    MaLoaiHoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiHoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiHoas", x => x.MaLoaiHoa);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "MauSanPhams",
                columns: table => new
                {
                    MaMau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMau = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaDip = table.Column<int>(type: "int", nullable: false),
                    KieuSanPham = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauSanPhams", x => x.MaMau);
                    table.ForeignKey(
                        name: "FK_MauSanPhams_DipSuDungs_MaDip",
                        column: x => x.MaDip,
                        principalTable: "DipSuDungs",
                        principalColumn: "MaDip",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaTuois",
                columns: table => new
                {
                    MaHoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaLoaiHoa = table.Column<int>(type: "int", nullable: false),
                    MauSac = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DonGiaThamKhao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaTuois", x => x.MaHoa);
                    table.ForeignKey(
                        name: "FK_HoaTuois_LoaiHoas_MaLoaiHoa",
                        column: x => x.MaLoaiHoa,
                        principalTable: "LoaiHoas",
                        principalColumn: "MaLoaiHoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKhachHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaKhachHang);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietMaus",
                columns: table => new
                {
                    MaChiTietMau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaMau = table.Column<int>(type: "int", nullable: false),
                    MaHoa = table.Column<int>(type: "int", nullable: false),
                    SoLuongTieuChuan = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietMaus", x => x.MaChiTietMau);
                    table.ForeignKey(
                        name: "FK_ChiTietMaus_HoaTuois_MaHoa",
                        column: x => x.MaHoa,
                        principalTable: "HoaTuois",
                        principalColumn: "MaHoa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietMaus_MauSanPhams_MaMau",
                        column: x => x.MaMau,
                        principalTable: "MauSanPhams",
                        principalColumn: "MaMau",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonDatHoas",
                columns: table => new
                {
                    MaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinhThucNhan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThoiGianNhanGiao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiGiao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NguoiNhan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoaiNguoiNhan = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    LoiNhan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhiGiaoHang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LyDoPhuPhi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LyDoHuy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDatHoas", x => x.MaDon);
                    table.ForeignKey(
                        name: "FK_DonDatHoas_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHoas",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    MaMau = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YeuCauRieng = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaChotGia = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHoas", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHoas_DonDatHoas_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonDatHoas",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHoas_MauSanPhams_MaMau",
                        column: x => x.MaMau,
                        principalTable: "MauSanPhams",
                        principalColumn: "MaMau",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGias_DonDatHoas_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonDatHoas",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGias_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiaoNhans",
                columns: table => new
                {
                    MaGiaoNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    HinhThuc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThoiGianDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianThucTe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NguoiGiao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoNhans", x => x.MaGiaoNhan);
                    table.ForeignKey(
                        name: "FK_GiaoNhans_DonDatHoas_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonDatHoas",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuXuLyDons",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    TrangThaiCu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TrangThaiMoi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiThucHien = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuXuLyDons", x => x.MaLichSu);
                    table.ForeignKey(
                        name: "FK_LichSuXuLyDons_DonDatHoas_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonDatHoas",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuXuLyDons_TaiKhoans_NguoiThucHien",
                        column: x => x.NguoiThucHien,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NguoiGhiNhan = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToans_DonDatHoas_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonDatHoas",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhToans_TaiKhoans_NguoiGhiNhan",
                        column: x => x.NguoiGhiNhan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHoas_MaDon",
                table: "ChiTietDonHoas",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHoas_MaMau",
                table: "ChiTietDonHoas",
                column: "MaMau");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMaus_MaHoa",
                table: "ChiTietMaus",
                column: "MaHoa");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMaus_MaMau_MaHoa",
                table: "ChiTietMaus",
                columns: new[] { "MaMau", "MaHoa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaDon",
                table: "DanhGias",
                column: "MaDon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaKhachHang",
                table: "DanhGias",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DipSuDungs_TenDip",
                table: "DipSuDungs",
                column: "TenDip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonDatHoas_MaKhachHang",
                table: "DonDatHoas",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoNhans_MaDon",
                table: "GiaoNhans",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_HoaTuois_MaLoaiHoa",
                table: "HoaTuois",
                column: "MaLoaiHoa");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_MaTaiKhoan",
                table: "KhachHangs",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyDons_MaDon",
                table: "LichSuXuLyDons",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyDons_NguoiThucHien",
                table: "LichSuXuLyDons",
                column: "NguoiThucHien");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiHoas_TenLoaiHoa",
                table: "LoaiHoas",
                column: "TenLoaiHoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MauSanPhams_MaDip",
                table: "MauSanPhams",
                column: "MaDip");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_MaDon",
                table: "ThanhToans",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_NguoiGhiNhan",
                table: "ThanhToans",
                column: "NguoiGhiNhan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHoas");

            migrationBuilder.DropTable(
                name: "ChiTietMaus");

            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "GiaoNhans");

            migrationBuilder.DropTable(
                name: "LichSuXuLyDons");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "HoaTuois");

            migrationBuilder.DropTable(
                name: "MauSanPhams");

            migrationBuilder.DropTable(
                name: "DonDatHoas");

            migrationBuilder.DropTable(
                name: "LoaiHoas");

            migrationBuilder.DropTable(
                name: "DipSuDungs");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
