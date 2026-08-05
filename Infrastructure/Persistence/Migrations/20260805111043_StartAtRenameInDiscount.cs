using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StartAtRenameInDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartAt",
                table: "Discounts",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "EndAt",
                table: "Discounts",
                newName: "EndsAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "Discounts",
                newName: "StartAt");

            migrationBuilder.RenameColumn(
                name: "EndsAt",
                table: "Discounts",
                newName: "EndAt");
        }
    }
}
