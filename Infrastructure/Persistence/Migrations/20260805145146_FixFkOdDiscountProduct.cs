using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixFkOdDiscountProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountProducts_Discounts_ProductId",
                table: "DiscountProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountProducts_Products_DiscountId",
                table: "DiscountProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountProducts_Discounts_DiscountId",
                table: "DiscountProducts",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountProducts_Products_ProductId",
                table: "DiscountProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountProducts_Discounts_DiscountId",
                table: "DiscountProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountProducts_Products_ProductId",
                table: "DiscountProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountProducts_Discounts_ProductId",
                table: "DiscountProducts",
                column: "ProductId",
                principalTable: "Discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountProducts_Products_DiscountId",
                table: "DiscountProducts",
                column: "DiscountId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
