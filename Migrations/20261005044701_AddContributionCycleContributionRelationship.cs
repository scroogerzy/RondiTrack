using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RondiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionCycleContributionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContributionCycle_Stokvels_StokvelId",
                table: "ContributionCycle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContributionCycle",
                table: "ContributionCycle");

            migrationBuilder.DropIndex(
                name: "IX_ContributionCycle_StokvelId",
                table: "ContributionCycle");

            migrationBuilder.RenameTable(
                name: "ContributionCycle",
                newName: "ContributionCycles");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ContributionCycles_StokvelId_PeriodNumber",
                table: "ContributionCycles",
                columns: new[] { "StokvelId", "PeriodNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContributionCycles",
                table: "ContributionCycles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StokvelId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cycle = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contributions_ContributionCycles_StokvelId_Cycle",
                        columns: x => new { x.StokvelId, x.Cycle },
                        principalTable: "ContributionCycles",
                        principalColumns: new[] { "StokvelId", "PeriodNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_StokvelId_Cycle",
                table: "Contributions",
                columns: new[] { "StokvelId", "Cycle" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContributionCycles_Stokvels_StokvelId",
                table: "ContributionCycles",
                column: "StokvelId",
                principalTable: "Stokvels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContributionCycles_Stokvels_StokvelId",
                table: "ContributionCycles");

            migrationBuilder.DropTable(
                name: "Contributions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ContributionCycles_StokvelId_PeriodNumber",
                table: "ContributionCycles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContributionCycles",
                table: "ContributionCycles");

            migrationBuilder.RenameTable(
                name: "ContributionCycles",
                newName: "ContributionCycle");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContributionCycle",
                table: "ContributionCycle",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ContributionCycle_StokvelId",
                table: "ContributionCycle",
                column: "StokvelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContributionCycle_Stokvels_StokvelId",
                table: "ContributionCycle",
                column: "StokvelId",
                principalTable: "Stokvels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
