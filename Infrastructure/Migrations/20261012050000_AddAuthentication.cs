using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RondiTrack.Infrastructure.Data;

#nullable disable

namespace RondiTrack.Infrastructure.Migrations;

[DbContext(typeof(RondiTrackDbContext))]
[Migration("20261012050000_AddAuthentication")]
public partial class AddAuthentication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordHash", table: "Users", type: "character varying(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "PlatformRole", table: "Users", type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "User");
        migrationBuilder.AddColumn<DateTime>(
            name: "PayoutProcessedAtUtc", table: "ContributionCycles", type: "timestamp with time zone", nullable: true);

        migrationBuilder.Sql("UPDATE \"Users\" SET \"Email\" = lower(trim(\"Email\"))");
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(name: "FK_RefreshTokens_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_TokenHash", table: "RefreshTokens", column: "TokenHash", unique: true);
        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_FamilyId", table: "RefreshTokens", column: "FamilyId");
        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_UserId", table: "RefreshTokens", column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RefreshTokens");
        migrationBuilder.DropIndex(name: "IX_Users_Email", table: "Users");
        migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
        migrationBuilder.DropColumn(name: "PlatformRole", table: "Users");
        migrationBuilder.DropColumn(name: "PayoutProcessedAtUtc", table: "ContributionCycles");
    }
}
