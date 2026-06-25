using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorIQ.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIr35Analysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MatchScore",
                table: "Jobs",
                type: "real",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ir35Analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    Verdict = table.Column<string>(type: "text", nullable: false),
                    SubstitutionScore = table.Column<int>(type: "integer", nullable: false),
                    ControlScore = table.Column<int>(type: "integer", nullable: false),
                    MooScore = table.Column<int>(type: "integer", nullable: false),
                    RedFlags = table.Column<string>(type: "text", nullable: false),
                    GreenFlags = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    SdcRisk = table.Column<string>(type: "text", nullable: false),
                    AnalysedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ir35Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ir35Analyses_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ir35Analyses_JobId",
                table: "Ir35Analyses",
                column: "JobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ir35Analyses");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "Jobs");
        }
    }
}
