using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class CorrectSLEAssessmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "AssessmentFeedback",
                table: "SLEs",
                newName: "BehaviourFeedback");

            migrationBuilder.AddColumn<string>(
                name: "AgreedAction",
                table: "SLEs",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessorPosition",
                table: "SLEs",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreedAction",
                table: "SLEs");

            migrationBuilder.DropColumn(
                name: "AssessorPosition",
                table: "SLEs");

            migrationBuilder.RenameColumn(
                name: "BehaviourFeedback",
                table: "SLEs",
                newName: "AssessmentFeedback");

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
    }
}
