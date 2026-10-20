using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class GioiHanMotPhieuGiaoNhanMoiDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GiaoNhans_MaDon",
                table: "GiaoNhans");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoNhans_MaDon",
                table: "GiaoNhans",
                column: "MaDon",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GiaoNhans_MaDon",
                table: "GiaoNhans");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoNhans_MaDon",
                table: "GiaoNhans",
                column: "MaDon");
        }
    }
}
