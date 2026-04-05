using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warcraft.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlizzardAccountId = table.Column<string>(type: "text", nullable: false),
                    BattleTag = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Realm = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    BlizzardCharId = table.Column<string>(type: "text", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Spec = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    TaskKey = table.Column<string>(type: "text", nullable: false),
                    TaskName = table.Column<string>(type: "text", nullable: false),
                    DayStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyTasks_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GearSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    SlotName = table.Column<string>(type: "text", nullable: false),
                    CurrentItem = table.Column<string>(type: "text", nullable: false),
                    BisItem = table.Column<string>(type: "text", nullable: false),
                    BisSource = table.Column<string>(type: "text", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GearSlots_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionCooldowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    CdKey = table.Column<string>(type: "text", nullable: false),
                    CdName = table.Column<string>(type: "text", nullable: false),
                    PeriodDays = table.Column<int>(type: "integer", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionCooldowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessionCooldowns_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    TaskKey = table.Column<string>(type: "text", nullable: false),
                    TaskName = table.Column<string>(type: "text", nullable: false),
                    TaskType = table.Column<string>(type: "text", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyTasks_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTasks_CharacterId_TaskKey_DayStart",
                table: "DailyTasks",
                columns: new[] { "CharacterId", "TaskKey", "DayStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GearSlots_CharacterId",
                table: "GearSlots",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionCooldowns_CharacterId_CdKey",
                table: "ProfessionCooldowns",
                columns: new[] { "CharacterId", "CdKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyTasks_CharacterId_TaskKey_WeekStart",
                table: "WeeklyTasks",
                columns: new[] { "CharacterId", "TaskKey", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyTasks");

            migrationBuilder.DropTable(
                name: "GearSlots");

            migrationBuilder.DropTable(
                name: "ProfessionCooldowns");

            migrationBuilder.DropTable(
                name: "WeeklyTasks");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
