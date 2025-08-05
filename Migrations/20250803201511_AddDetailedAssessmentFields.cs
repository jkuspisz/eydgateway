using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedAssessmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearningObjectives",
                table: "SLEs");

            migrationBuilder.AddColumn<string>(
                name: "AreasForImprovement",
                table: "SLEs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreasOfStrength",
                table: "SLEs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackComments",
                table: "SLEs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendForProgression",
                table: "SLEs",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreasForImprovement",
                table: "SLEs");

            migrationBuilder.DropColumn(
                name: "AreasOfStrength",
                table: "SLEs");

            migrationBuilder.DropColumn(
                name: "FeedbackComments",
                table: "SLEs");

            migrationBuilder.DropColumn(
                name: "RecommendForProgression",
                table: "SLEs");

            migrationBuilder.AddColumn<string>(
                name: "LearningObjectives",
                table: "SLEs",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
