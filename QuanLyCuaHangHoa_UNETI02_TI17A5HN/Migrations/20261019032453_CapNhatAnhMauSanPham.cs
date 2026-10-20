using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatAnhMauSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE dbo.MauSanPhams
                SET HinhAnh = '/images/mau/mau27.jpg'
                WHERE MaMau = 27;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
