using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraniteAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_Products_ProductId",
                table: "Galleries");

            migrationBuilder.DropIndex(
                name: "IX_Galleries_ProductId",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Galleries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Galleries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_ProductId",
                table: "Galleries",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_Products_ProductId",
                table: "Galleries",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
