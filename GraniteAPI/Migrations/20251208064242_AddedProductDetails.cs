using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraniteAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedProductDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Products",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "Color");
        }
    }
}
