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
            // Remove duplicate portal email links before creating unique indexes
            migrationBuilder.Sql(
                @"WITH duplicates AS (
                        SELECT PortalUserEmail, MIN(Id) AS KeepId
                        FROM Suppliers
                        WHERE PortalUserEmail IS NOT NULL
                        GROUP BY PortalUserEmail
                        HAVING COUNT(*) > 1
                    )
                    DELETE s
                    FROM Suppliers s
                    INNER JOIN duplicates d ON s.PortalUserEmail = d.PortalUserEmail
                    WHERE s.Id <> d.KeepId;");

            // Remove duplicate portal user links before creating unique indexes
            migrationBuilder.Sql(
                @"WITH duplicates AS (
                        SELECT PortalUserId, MIN(Id) AS KeepId
                        FROM Suppliers
                        WHERE PortalUserId IS NOT NULL
                        GROUP BY PortalUserId
                        HAVING COUNT(*) > 1
                    )
                    DELETE s
                    FROM Suppliers s
                    INNER JOIN duplicates d ON s.PortalUserId = d.PortalUserId
                    WHERE s.Id <> d.KeepId;");

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
