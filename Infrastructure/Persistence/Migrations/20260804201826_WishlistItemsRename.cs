using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WishlistItemsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wishlist_items_Products_ProductId",
                table: "wishlist_items");

            migrationBuilder.DropForeignKey(
                name: "FK_wishlist_items_Wishlist_WishlistId",
                table: "wishlist_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wishlist_items",
                table: "wishlist_items");

            migrationBuilder.RenameTable(
                name: "wishlist_items",
                newName: "WishlistItems");

            migrationBuilder.RenameIndex(
                name: "IX_wishlist_items_WishlistId_ProductId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_WishlistId_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_wishlist_items_ProductId",
                table: "WishlistItems",
                newName: "IX_WishlistItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WishlistItems",
                table: "WishlistItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Products_ProductId",
                table: "WishlistItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Wishlist_WishlistId",
                table: "WishlistItems",
                column: "WishlistId",
                principalTable: "Wishlist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Products_ProductId",
                table: "WishlistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Wishlist_WishlistId",
                table: "WishlistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WishlistItems",
                table: "WishlistItems");

            migrationBuilder.RenameTable(
                name: "WishlistItems",
                newName: "wishlist_items");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_WishlistId_ProductId",
                table: "wishlist_items",
                newName: "IX_wishlist_items_WishlistId_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_WishlistItems_ProductId",
                table: "wishlist_items",
                newName: "IX_wishlist_items_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wishlist_items",
                table: "wishlist_items",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_wishlist_items_Products_ProductId",
                table: "wishlist_items",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wishlist_items_Wishlist_WishlistId",
                table: "wishlist_items",
                column: "WishlistId",
                principalTable: "Wishlist",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
