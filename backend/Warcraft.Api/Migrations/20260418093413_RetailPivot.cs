using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warcraft.Api.Migrations
{
    /// <inheritdoc />
    public partial class RetailPivot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "WeeklyTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemLevel",
                table: "GearSlots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "GearSlots",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemLevelAverage",
                table: "Characters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DungeonTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MythicPlusRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    DungeonKey = table.Column<string>(type: "text", nullable: false),
                    DungeonName = table.Column<string>(type: "text", nullable: false),
                    KeyLevel = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MythicPlusRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MythicPlusRuns_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionCdTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PeriodDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionCdTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaidTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RaidName = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    BossCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyQuestTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    QuestType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyQuestTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DungeonTemplates_Key",
                table: "DungeonTemplates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MythicPlusRuns_CharacterId_WeekStart",
                table: "MythicPlusRuns",
                columns: new[] { "CharacterId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionCdTemplates_Key",
                table: "ProfessionCdTemplates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaidTemplates_Key",
                table: "RaidTemplates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyQuestTemplates_Key",
                table: "WeeklyQuestTemplates",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DungeonTemplates");

            migrationBuilder.DropTable(
                name: "MythicPlusRuns");

            migrationBuilder.DropTable(
                name: "ProfessionCdTemplates");

            migrationBuilder.DropTable(
                name: "RaidTemplates");

            migrationBuilder.DropTable(
                name: "WeeklyQuestTemplates");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "WeeklyTasks");

            migrationBuilder.DropColumn(
                name: "ItemLevel",
                table: "GearSlots");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "GearSlots");

            migrationBuilder.DropColumn(
                name: "ItemLevelAverage",
                table: "Characters");
        }
    }
}
