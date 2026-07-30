using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Mappings
{
    /// <inheritdoc />
    public partial class CustomerDeletedUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Role_Valid_Values",
                table: "Users");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Role_Valid_Values",
                table: "Users",
                sql: "[Role] IN (0,1,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Role_Valid_Values",
                table: "Users");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Role_Valid_Values",
                table: "Users",
                sql: "[Role] IN (0,1,2,3)");
        }
    }
}
