using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentUrlAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyKeys_UserId_Key",
                table: "IdempotencyKeys");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "IdempotencyKeys",
                newName: "ResourceId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdempotencyOperation",
                table: "IdempotencyKeys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_UserId_Key_IdempotencyOperation",
                table: "IdempotencyKeys",
                columns: new[] { "UserId", "Key", "IdempotencyOperation" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyKeys_UserId_Key_IdempotencyOperation",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IdempotencyOperation",
                table: "IdempotencyKeys");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "IdempotencyKeys",
                newName: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_UserId_Key",
                table: "IdempotencyKeys",
                columns: new[] { "UserId", "Key" },
                unique: true);
        }
    }
}
