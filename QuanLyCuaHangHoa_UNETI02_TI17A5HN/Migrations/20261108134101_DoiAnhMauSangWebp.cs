using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyCuaHangHoa_UNETI02_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class DoiAnhMauSangWebp : Migration
    {
        // Các ảnh mẫu đã được thay bằng file .webp trong wwwroot/images/mau
        private static readonly string[] AnhDoiSangWebp =
        {
            "mau07", "mau08", "mau11", "mau12", "mau13", "mau14", "mau15", "mau16", "mau17",
            "mau18", "mau19", "mau20", "mau21", "mau22", "mau23", "mau24", "mau25"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var ten in AnhDoiSangWebp)
            {
                migrationBuilder.Sql(
                    $"UPDATE MauSanPhams SET HinhAnh = '/images/mau/{ten}.webp' WHERE HinhAnh = '/images/mau/{ten}.jpg';");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var ten in AnhDoiSangWebp)
            {
                migrationBuilder.Sql(
                    $"UPDATE MauSanPhams SET HinhAnh = '/images/mau/{ten}.jpg' WHERE HinhAnh = '/images/mau/{ten}.webp';");
            }
        }
    }
}