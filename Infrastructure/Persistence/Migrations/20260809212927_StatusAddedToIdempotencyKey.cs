using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StatusAddedToIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "IdempotencyKeys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseStatusCode",
                table: "IdempotencyKeys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "IdempotencyKeys",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "ResponseStatusCode",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IdempotencyKeys");
        }
    }
}
