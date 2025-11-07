using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraniteAPI.Migrations
{
    /// <inheritdoc />
    public partial class imagefilechanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Products",
                newName: "ImageFileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageFileName",
                table: "Products",
                newName: "ImageUrl");
        }
    }
}
