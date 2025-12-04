using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementSubRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProcurementCanCreate",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcurementCanDelete",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcurementCanEdit",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcurementCanView",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementSubRole",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcurementCanCreate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProcurementCanDelete",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProcurementCanEdit",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProcurementCanView",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProcurementSubRole",
                table: "AspNetUsers");
        }
    }
}
