using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RondiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionMemberCycleUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Contributions_StokvelId_UserId_Cycle",
                table: "Contributions",
                columns: new[] { "StokvelId", "UserId", "Cycle" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Contributions_StokvelId_UserId_Cycle",
                table: "Contributions");
        }
    }
}
