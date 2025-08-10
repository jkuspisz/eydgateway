using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddFRCPSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FRCPReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ESStatus = table.Column<int>(type: "integer", nullable: false),
                    EYDStatus = table.Column<int>(type: "integer", nullable: false),
                    PanelStatus = table.Column<int>(type: "integer", nullable: false),
                    ESLocked = table.Column<bool>(type: "boolean", nullable: false),
                    EYDLocked = table.Column<bool>(type: "boolean", nullable: false),
                    PanelLocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRCPReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FRCPReviews_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FRCPESAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    EPAId = table.Column<int>(type: "integer", nullable: false),
                    EntrustmentLevel = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRCPESAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FRCPESAssessments_EPAs_EPAId",
                        column: x => x.EPAId,
                        principalTable: "EPAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FRCPESAssessments_FRCPReviews_FRCPReviewId",
                        column: x => x.FRCPReviewId,
                        principalTable: "FRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FRCPEYDReflections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    ProgressSummary = table.Column<string>(type: "text", nullable: false),
                    ChallengesFaced = table.Column<string>(type: "text", nullable: false),
                    LearningGoals = table.Column<string>(type: "text", nullable: false),
                    SupportNeeded = table.Column<string>(type: "text", nullable: false),
                    ReadyForNextStage = table.Column<bool>(type: "boolean", nullable: false),
                    ReadyForNextStageExplanation = table.Column<string>(type: "text", nullable: false),
                    AdditionalTrainingNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    AdditionalTrainingDetails = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRCPEYDReflections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FRCPEYDReflections_FRCPReviews_FRCPReviewId",
                        column: x => x.FRCPReviewId,
                        principalTable: "FRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FRCPPanelReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    OverallAssessment = table.Column<string>(type: "text", nullable: false),
                    RecommendedActions = table.Column<string>(type: "text", nullable: false),
                    ProgressPlan = table.Column<string>(type: "text", nullable: false),
                    NextReviewDate = table.Column<string>(type: "text", nullable: false),
                    PanelConsensusReached = table.Column<bool>(type: "boolean", nullable: false),
                    ConsensusNotes = table.Column<string>(type: "text", nullable: false),
                    FinalDecision = table.Column<string>(type: "text", nullable: false),
                    DecisionRationale = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRCPPanelReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FRCPPanelReviews_FRCPReviews_FRCPReviewId",
                        column: x => x.FRCPReviewId,
                        principalTable: "FRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FRCPESAssessments_EPAId",
                table: "FRCPESAssessments",
                column: "EPAId");

            migrationBuilder.CreateIndex(
                name: "IX_FRCPESAssessments_FRCPReviewId",
                table: "FRCPESAssessments",
                column: "FRCPReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_FRCPEYDReflections_FRCPReviewId",
                table: "FRCPEYDReflections",
                column: "FRCPReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FRCPPanelReviews_FRCPReviewId",
                table: "FRCPPanelReviews",
                column: "FRCPReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FRCPReviews_EYDUserId",
                table: "FRCPReviews",
                column: "EYDUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FRCPESAssessments");

            migrationBuilder.DropTable(
                name: "FRCPEYDReflections");

            migrationBuilder.DropTable(
                name: "FRCPPanelReviews");

            migrationBuilder.DropTable(
                name: "FRCPReviews");
        }
    }
}
