using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorIQ.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlertIr35Preference",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlertKeywords",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AlertMinDayRate",
                table: "Profiles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<float>(
                name: "AlertMinMatchScore",
                table: "Profiles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "AlertsEnabled",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAlertSentAt",
                table: "Profiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertIr35Preference",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AlertKeywords",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AlertMinDayRate",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AlertMinMatchScore",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AlertsEnabled",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "LastAlertSentAt",
                table: "Profiles");
        }
    }
}
