using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractorIQ.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchScoreToJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MatchScore",
                table: "Jobs",
                type: "real",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "Jobs");
        }
    }
}
