using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierBidsAndBidReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[SupplierBids]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[SupplierBids](
                        [Id] uniqueidentifier NOT NULL,
                        [RfxId] uniqueidentifier NOT NULL,
                        [SubmittedByUserId] nvarchar(450) NOT NULL,
                        [BidAmount] decimal(18,2) NOT NULL,
                        [Currency] nvarchar(max) NOT NULL,
                        [ExpectedDeliveryDate] datetime2 NULL,
                        [ProposalSummary] nvarchar(max) NOT NULL,
                        [Notes] nvarchar(max) NULL,
                        [DocumentsJson] nvarchar(max) NOT NULL,
                        [InputsJson] nvarchar(max) NOT NULL,
                        [SubmittedAtUtc] datetime2 NOT NULL,
                        CONSTRAINT [PK_SupplierBids] PRIMARY KEY ([Id])
                    );

                    ALTER TABLE [dbo].[SupplierBids] ADD CONSTRAINT [FK_SupplierBids_Rfxes_RfxId]
                        FOREIGN KEY ([RfxId]) REFERENCES [dbo].[Rfxes]([Id]) ON DELETE CASCADE;

                    ALTER TABLE [dbo].[SupplierBids] ADD CONSTRAINT [FK_SupplierBids_AspNetUsers_SubmittedByUserId]
                        FOREIGN KEY ([SubmittedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;

                    CREATE INDEX [IX_SupplierBids_RfxId] ON [dbo].[SupplierBids] ([RfxId]);
                    CREATE INDEX [IX_SupplierBids_SubmittedByUserId] ON [dbo].[SupplierBids] ([SubmittedByUserId]);
                END
                """);

            migrationBuilder.CreateTable(
                name: "BidReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BidReviews_AspNetUsers_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BidReviews_SupplierBids_BidId",
                        column: x => x.BidId,
                        principalTable: "SupplierBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BidReviews_BidId",
                table: "BidReviews",
                column: "BidId");

            migrationBuilder.CreateIndex(
                name: "IX_BidReviews_ReviewerUserId",
                table: "BidReviews",
                column: "ReviewerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BidReviews");
        }
    }
}
