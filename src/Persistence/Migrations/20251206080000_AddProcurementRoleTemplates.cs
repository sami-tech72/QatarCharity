using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementRoleTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcurementPermissionDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DefaultRead = table.Column<bool>(type: "bit", nullable: false),
                    DefaultWrite = table.Column<bool>(type: "bit", nullable: false),
                    DefaultCreate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementPermissionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRoleTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    TotalUsers = table.Column<int>(type: "int", nullable: false),
                    NewUsers = table.Column<int>(type: "int", nullable: false),
                    ExtraCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRoleTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRoleAvatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProcurementRoleTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRoleAvatars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRoleAvatars_ProcurementRoleTemplates_ProcurementRoleTemplateId",
                        column: x => x.ProcurementRoleTemplateId,
                        principalTable: "ProcurementRoleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcurementRoleTemplateId = table.Column<int>(type: "int", nullable: false),
                    ProcurementPermissionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanWrite = table.Column<bool>(type: "bit", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRolePermissions_ProcurementPermissionDefinitions_ProcurementPermissionDefinitionId",
                        column: x => x.ProcurementPermissionDefinitionId,
                        principalTable: "ProcurementPermissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementRolePermissions_ProcurementRoleTemplates_ProcurementRoleTemplateId",
                        column: x => x.ProcurementRoleTemplateId,
                        principalTable: "ProcurementRoleTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRoleAvatars_ProcurementRoleTemplateId",
                table: "ProcurementRoleAvatars",
                column: "ProcurementRoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRolePermissions_ProcurementPermissionDefinitionId",
                table: "ProcurementRolePermissions",
                column: "ProcurementPermissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRolePermissions_ProcurementRoleTemplateId",
                table: "ProcurementRolePermissions",
                column: "ProcurementRoleTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementRoleAvatars");

            migrationBuilder.DropTable(
                name: "ProcurementRolePermissions");

            migrationBuilder.DropTable(
                name: "ProcurementPermissionDefinitions");

            migrationBuilder.DropTable(
                name: "ProcurementRoleTemplates");
        }
    }
}
