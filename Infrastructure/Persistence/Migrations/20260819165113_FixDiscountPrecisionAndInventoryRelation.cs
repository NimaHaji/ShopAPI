using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDiscountPrecisionAndInventoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_ProductVariants_ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems");

            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantImage",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderItemOption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemOption_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOption_OrderItemId",
                table: "OrderItemOption",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemOption");

            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantImage",
                table: "OrderItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems",
                column: "ProductVariantId1",
                unique: true,
                filter: "[ProductVariantId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_ProductVariants_ProductVariantId1",
                schema: "dbo",
                table: "InventoryItems",
                column: "ProductVariantId1",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }
    }
}
