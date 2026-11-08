using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RondiTrack.Infrastructure.Data;

#nullable disable

namespace Infrastructure.Migrations;

[DbContext(typeof(RondiTrackDbContext))]
[Migration("20261107090000_AddAssignment91Entities")]
public partial class AddAssignment91Entities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Rules", table: "Stokvels", type: "character varying(4000)", maxLength: 4000, nullable: false, defaultValue: "");
        migrationBuilder.CreateTable(
            name: "StokvelInvites",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StokvelId = table.Column<Guid>(type: "uuid", nullable: false),
                CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StokvelInvites", x => x.Id);
                table.ForeignKey("FK_StokvelInvites_Stokvels_StokvelId", x => x.StokvelId, "Stokvels", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_StokvelInvites_CodeHash", "StokvelInvites", "CodeHash", unique: true);
        migrationBuilder.CreateIndex("IX_StokvelInvites_StokvelId_ExpiresAtUtc", "StokvelInvites", new[] { "StokvelId", "ExpiresAtUtc" });

        migrationBuilder.CreateTable(
            name: "CycleCloseReceipts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StokvelId = table.Column<Guid>(type: "uuid", nullable: false),
                CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                PreviewVersion = table.Column<long>(type: "bigint", nullable: false),
                ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CycleCloseReceipts", x => x.Id);
                table.ForeignKey("FK_CycleCloseReceipts_Stokvels_StokvelId", x => x.StokvelId, "Stokvels", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_CycleCloseReceipts_ContributionCycles_CycleId", x => x.CycleId, "ContributionCycles", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_CycleCloseReceipts_IdempotencyKey", "CycleCloseReceipts", "IdempotencyKey", unique: true);
        migrationBuilder.CreateIndex("IX_CycleCloseReceipts_CycleId", "CycleCloseReceipts", "CycleId", unique: true);
        migrationBuilder.CreateIndex("IX_CycleCloseReceipts_StokvelId", "CycleCloseReceipts", "StokvelId");

        migrationBuilder.CreateTable(
            name: "ContributionProofs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StokvelId = table.Column<Guid>(type: "uuid", nullable: false),
                ContributionId = table.Column<Guid>(type: "uuid", nullable: false),
                UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Content = table.Column<byte[]>(type: "bytea", nullable: false),
                UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContributionProofs", x => x.Id);
                table.ForeignKey("FK_ContributionProofs_Contributions_ContributionId", x => x.ContributionId, "Contributions", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ContributionProofs_Stokvels_StokvelId", x => x.StokvelId, "Stokvels", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ContributionProofs_Users_UploadedByUserId", x => x.UploadedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_ContributionProofs_ContributionId", "ContributionProofs", "ContributionId", unique: true);
        migrationBuilder.CreateIndex("IX_ContributionProofs_StokvelId", "ContributionProofs", "StokvelId");
        migrationBuilder.CreateIndex("IX_ContributionProofs_UploadedByUserId", "ContributionProofs", "UploadedByUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Rules", table: "Stokvels");
        migrationBuilder.DropTable("ContributionProofs");
        migrationBuilder.DropTable("CycleCloseReceipts");
        migrationBuilder.DropTable("StokvelInvites");
    }
}
