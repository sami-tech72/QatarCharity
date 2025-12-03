using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPortalUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_PortalUserId",
                table: "Suppliers");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PortalUserEmail",
                table: "Suppliers",
                column: "PortalUserEmail",
                unique: true,
                filter: "[PortalUserEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PortalUserId",
                table: "Suppliers",
                column: "PortalUserId",
                unique: true,
                filter: "[PortalUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_PortalUserEmail",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_PortalUserId",
                table: "Suppliers");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PortalUserId",
                table: "Suppliers",
                column: "PortalUserId");
        }
    }
}
