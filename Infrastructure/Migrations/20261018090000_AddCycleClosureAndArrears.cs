using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RondiTrack.Infrastructure.Data;

#nullable disable

namespace RondiTrack.Infrastructure.Migrations;

[DbContext(typeof(RondiTrackDbContext))]
[Migration("20261018090000_AddCycleClosureAndArrears")]
public partial class AddCycleClosureAndArrears : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "ClosedAtUtc",
            table: "ContributionCycles",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ArrearRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StokvelId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CyclePeriodNumber = table.Column<int>(type: "integer", nullable: false),
                ShortfallCents = table.Column<long>(type: "bigint", nullable: false),
                RecordedAtUtc = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ArrearRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_ArrearRecords_Stokvels_StokvelId",
                    column: x => x.StokvelId,
                    principalTable: "Stokvels",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ArrearRecords_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "UX_ArrearRecords_StokvelId_UserId_CyclePeriodNumber",
            table: "ArrearRecords",
            columns: new[] { "StokvelId", "UserId", "CyclePeriodNumber" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ArrearRecords");

        migrationBuilder.DropColumn(
            name: "ClosedAtUtc",
            table: "ContributionCycles");
    }
}
