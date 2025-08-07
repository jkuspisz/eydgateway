using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPSQSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_SignificantEvents_SignificantEventId",
                table: "EPAMappings");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_SignificantEventId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "SignificantEventId",
                table: "EPAMappings");

            migrationBuilder.CreateTable(
                name: "PSQQuestionnaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UniqueCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PSQQuestionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PSQQuestionnaires_AspNetUsers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PSQResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PSQQuestionnaireId = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PutMeAtEaseScore = table.Column<int>(type: "integer", nullable: true),
                    TreatedWithDignityScore = table.Column<int>(type: "integer", nullable: true),
                    ListenedToConcernsScore = table.Column<int>(type: "integer", nullable: true),
                    ExplainedTreatmentOptionsScore = table.Column<int>(type: "integer", nullable: true),
                    InvolvedInDecisionsScore = table.Column<int>(type: "integer", nullable: true),
                    InvolvedFamilyScore = table.Column<int>(type: "integer", nullable: true),
                    TailoredApproachScore = table.Column<int>(type: "integer", nullable: true),
                    ExplainedNextStepsScore = table.Column<int>(type: "integer", nullable: true),
                    ProvidedGuidanceScore = table.Column<int>(type: "integer", nullable: true),
                    AllocatedTimeScore = table.Column<int>(type: "integer", nullable: true),
                    WorkedWithTeamScore = table.Column<int>(type: "integer", nullable: true),
                    CanTrustDentistScore = table.Column<int>(type: "integer", nullable: true),
                    DoesWellComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CouldImproveComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PSQResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PSQResponses_PSQQuestionnaires_PSQQuestionnaireId",
                        column: x => x.PSQQuestionnaireId,
                        principalTable: "PSQQuestionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PSQQuestionnaires_PerformerId",
                table: "PSQQuestionnaires",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_PSQResponses_PSQQuestionnaireId",
                table: "PSQResponses",
                column: "PSQQuestionnaireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PSQResponses");

            migrationBuilder.DropTable(
                name: "PSQQuestionnaires");

            migrationBuilder.AddColumn<int>(
                name: "SignificantEventId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_SignificantEventId",
                table: "EPAMappings",
                column: "SignificantEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_SignificantEvents_SignificantEventId",
                table: "EPAMappings",
                column: "SignificantEventId",
                principalTable: "SignificantEvents",
                principalColumn: "Id");
        }
    }
}
