using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddMSFSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MSFQuestionnaires",
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
                    table.PrimaryKey("PK_MSFQuestionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSFQuestionnaires_AspNetUsers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MSFResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MSFQuestionnaireId = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TreatWithCompassionScore = table.Column<int>(type: "integer", nullable: true),
                    EnableInformedDecisionsScore = table.Column<int>(type: "integer", nullable: true),
                    RecogniseCommunicationNeedsScore = table.Column<int>(type: "integer", nullable: true),
                    ProduceClearCommunicationsScore = table.Column<int>(type: "integer", nullable: true),
                    DemonstrateIntegrityScore = table.Column<int>(type: "integer", nullable: true),
                    WorkWithinScopeScore = table.Column<int>(type: "integer", nullable: true),
                    EngageWithDevelopmentScore = table.Column<int>(type: "integer", nullable: true),
                    KeepPracticeUpToDateScore = table.Column<int>(type: "integer", nullable: true),
                    FacilitateLearningScore = table.Column<int>(type: "integer", nullable: true),
                    InteractWithColleaguesScore = table.Column<int>(type: "integer", nullable: true),
                    PromoteEqualityScore = table.Column<int>(type: "integer", nullable: true),
                    RecogniseImpactOfBehavioursScore = table.Column<int>(type: "integer", nullable: true),
                    ManageTimeAndResourcesScore = table.Column<int>(type: "integer", nullable: true),
                    WorkAsTeamMemberScore = table.Column<int>(type: "integer", nullable: true),
                    WorkToStandardsScore = table.Column<int>(type: "integer", nullable: true),
                    ParticipateInImprovementScore = table.Column<int>(type: "integer", nullable: true),
                    MinimiseWasteScore = table.Column<int>(type: "integer", nullable: true),
                    DoesWellComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CouldImproveComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSFResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSFResponses_MSFQuestionnaires_MSFQuestionnaireId",
                        column: x => x.MSFQuestionnaireId,
                        principalTable: "MSFQuestionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MSFQuestionnaires_PerformerId",
                table: "MSFQuestionnaires",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_MSFResponses_MSFQuestionnaireId",
                table: "MSFResponses",
                column: "MSFQuestionnaireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MSFResponses");

            migrationBuilder.DropTable(
                name: "MSFQuestionnaires");
        }
    }
}
