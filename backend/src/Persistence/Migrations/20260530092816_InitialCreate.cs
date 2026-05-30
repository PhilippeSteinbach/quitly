using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quitly.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "triggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_triggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "habits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    StartedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_habits_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassivePromptEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PromptTone = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_reminders_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "streaks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "integer", nullable: false),
                    LastAbstinentDay = table.Column<DateOnly>(type: "date", nullable: true),
                    LastNonAbstinentDay = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streaks", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_streaks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_insights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckInCount = table.Column<int>(type: "integer", nullable: false),
                    AbstinentDays = table.Column<int>(type: "integer", nullable: false),
                    TopTriggers = table.Column<string>(type: "jsonb", nullable: false),
                    MoodTrend = table.Column<string>(type: "jsonb", nullable: false),
                    SummaryText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_insights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weekly_insights_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_ins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Mood = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_ins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_check_ins_habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_check_ins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relapses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ContextNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relapses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_relapses_habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_relapses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_in_triggers",
                columns: table => new
                {
                    CheckInId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_in_triggers", x => new { x.CheckInId, x.TriggerId });
                    table.ForeignKey(
                        name: "FK_check_in_triggers_check_ins_CheckInId",
                        column: x => x.CheckInId,
                        principalTable: "check_ins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_check_in_triggers_triggers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "triggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recovery_plan_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelapseId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DueWithinHours = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_plan_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recovery_plan_steps_relapses_RelapseId",
                        column: x => x.RelapseId,
                        principalTable: "relapses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recovery_plan_steps_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_check_in_triggers_TriggerId",
                table: "check_in_triggers",
                column: "TriggerId");

            migrationBuilder.CreateIndex(
                name: "IX_check_ins_HabitId",
                table: "check_ins",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_check_ins_UserId_Day",
                table: "check_ins",
                columns: new[] { "UserId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_habits_UserId_Active",
                table: "habits",
                columns: new[] { "UserId", "Active" },
                unique: true,
                filter: "active = true");

            migrationBuilder.CreateIndex(
                name: "IX_recovery_plan_steps_RelapseId",
                table: "recovery_plan_steps",
                column: "RelapseId");

            migrationBuilder.CreateIndex(
                name: "IX_recovery_plan_steps_UserId",
                table: "recovery_plan_steps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_relapses_HabitId",
                table: "relapses",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_relapses_UserId",
                table: "relapses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_triggers_Code",
                table: "triggers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_insights_UserId_WeekStart",
                table: "weekly_insights",
                columns: new[] { "UserId", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_in_triggers");

            migrationBuilder.DropTable(
                name: "recovery_plan_steps");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "streaks");

            migrationBuilder.DropTable(
                name: "weekly_insights");

            migrationBuilder.DropTable(
                name: "check_ins");

            migrationBuilder.DropTable(
                name: "triggers");

            migrationBuilder.DropTable(
                name: "relapses");

            migrationBuilder.DropTable(
                name: "habits");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
