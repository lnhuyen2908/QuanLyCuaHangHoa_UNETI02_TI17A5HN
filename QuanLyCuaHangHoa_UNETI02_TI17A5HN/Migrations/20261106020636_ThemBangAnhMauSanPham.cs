using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangAnhMauSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnhMauSanPhams",
                columns: table => new
                {
                    MaAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaMau = table.Column<int>(type: "int", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhMauSanPhams", x => x.MaAnh);
                    table.ForeignKey(
                        name: "FK_AnhMauSanPhams_MauSanPhams_MaMau",
                        column: x => x.MaMau,
                        principalTable: "MauSanPhams",
                        principalColumn: "MaMau",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhMauSanPhams_MaMau",
                table: "AnhMauSanPhams",
                column: "MaMau");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhMauSanPhams");
        }
    }
}
