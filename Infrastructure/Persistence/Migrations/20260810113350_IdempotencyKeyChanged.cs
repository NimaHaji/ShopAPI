using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdempotencyKeyChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "ResponseStatusCode",
                table: "IdempotencyKeys");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "IdempotencyKeys",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "IdempotencyKeys");

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
        }
    }
}
