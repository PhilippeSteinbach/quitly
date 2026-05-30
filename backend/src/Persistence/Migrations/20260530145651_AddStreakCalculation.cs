using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quitly.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_streaks_users_UserId",
                table: "streaks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streaks",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "CurrentStreakDays",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "LastAbstinentDay",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "LastNonAbstinentDay",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "ContextNote",
                table: "relapses");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "streaks",
                newName: "LastSyncAt");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "streaks",
                newName: "HabitId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "streaks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "CurrentStreakSeconds",
                table: "streaks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LastServerUtcMs",
                table: "streaks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<byte[]>(
                name: "ContextNoteEncrypted",
                table: "relapses",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviousStreakSeconds",
                table: "relapses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "habits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_streaks",
                table: "streaks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_streaks_HabitId",
                table: "streaks",
                column: "HabitId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_streaks_habits_HabitId",
                table: "streaks",
                column: "HabitId",
                principalTable: "habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_streaks_habits_HabitId",
                table: "streaks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streaks",
                table: "streaks");

            migrationBuilder.DropIndex(
                name: "IX_streaks_HabitId",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "CurrentStreakSeconds",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "LastServerUtcMs",
                table: "streaks");

            migrationBuilder.DropColumn(
                name: "ContextNoteEncrypted",
                table: "relapses");

            migrationBuilder.DropColumn(
                name: "PreviousStreakSeconds",
                table: "relapses");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "habits");

            migrationBuilder.RenameColumn(
                name: "LastSyncAt",
                table: "streaks",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "HabitId",
                table: "streaks",
                newName: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStreakDays",
                table: "streaks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastAbstinentDay",
                table: "streaks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastNonAbstinentDay",
                table: "streaks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextNote",
                table: "relapses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_streaks",
                table: "streaks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_streaks_users_UserId",
                table: "streaks",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
