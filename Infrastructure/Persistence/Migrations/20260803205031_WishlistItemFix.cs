using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WishlistItemFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wishlist_items_Wishlist_Id",
                table: "wishlist_items");

            migrationBuilder.AddForeignKey(
                name: "FK_wishlist_items_Wishlist_WishlistId",
                table: "wishlist_items",
                column: "WishlistId",
                principalTable: "Wishlist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wishlist_items_Wishlist_WishlistId",
                table: "wishlist_items");

            migrationBuilder.AddForeignKey(
                name: "FK_wishlist_items_Wishlist_Id",
                table: "wishlist_items",
                column: "Id",
                principalTable: "Wishlist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
