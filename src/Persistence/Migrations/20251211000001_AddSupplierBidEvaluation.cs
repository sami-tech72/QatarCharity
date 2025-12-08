using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierBidEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvaluatedByUserId",
                table: "SupplierBids",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EvaluatedAtUtc",
                table: "SupplierBids",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluationNotes",
                table: "SupplierBids",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvaluationStatus",
                table: "SupplierBids",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pending Review");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluatedByUserId",
                table: "SupplierBids");

            migrationBuilder.DropColumn(
                name: "EvaluatedAtUtc",
                table: "SupplierBids");

            migrationBuilder.DropColumn(
                name: "EvaluationNotes",
                table: "SupplierBids");

            migrationBuilder.DropColumn(
                name: "EvaluationStatus",
                table: "SupplierBids");
        }
    }
}
