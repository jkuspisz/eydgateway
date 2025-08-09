using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddIRCPWorkflowSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterimReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "text", nullable: false),
                    ESUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ESCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EYDCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PanelCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsESCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsEYDCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPanelCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ESOverallAssessment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESStrengths = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESAreasForDevelopment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESRecommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESAdditionalComments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESProgressSinceLastReview = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESClinicalPerformance = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ESProfessionalBehavior = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EYDReflectionComments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EYDLearningGoals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EYDActionPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PanelComments = table.Column<string>(type: "text", nullable: true),
                    PanelRecommendations = table.Column<string>(type: "text", nullable: true),
                    PanelOutcome = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterimReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterimReviews_AspNetUsers_ESUserId",
                        column: x => x.ESUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InterimReviews_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IRCPReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "text", nullable: false),
                    EYDUserName = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ESStatus = table.Column<int>(type: "integer", nullable: false),
                    EYDStatus = table.Column<int>(type: "integer", nullable: false),
                    PanelStatus = table.Column<int>(type: "integer", nullable: false),
                    ESLocked = table.Column<bool>(type: "boolean", nullable: false),
                    EYDLocked = table.Column<bool>(type: "boolean", nullable: false),
                    PanelLocked = table.Column<bool>(type: "boolean", nullable: false),
                    ESSubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EYDSubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PanelSubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ESSubmittedBy = table.Column<string>(type: "text", nullable: true),
                    EYDSubmittedBy = table.Column<string>(type: "text", nullable: true),
                    PanelSubmittedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IRCPEPAAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InterimReviewId = table.Column<int>(type: "integer", nullable: false),
                    EPAId = table.Column<int>(type: "integer", nullable: false),
                    LevelOfEntrustment = table.Column<int>(type: "integer", nullable: true),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPEPAAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRCPEPAAssessments_EPAs_EPAId",
                        column: x => x.EPAId,
                        principalTable: "EPAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IRCPEPAAssessments_InterimReviews_InterimReviewId",
                        column: x => x.InterimReviewId,
                        principalTable: "InterimReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IRCPESAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    EPACode = table.Column<string>(type: "text", nullable: false),
                    EntrustmentLevel = table.Column<int>(type: "integer", nullable: true),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPESAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRCPESAssessments_IRCPReviews_IRCPReviewId",
                        column: x => x.IRCPReviewId,
                        principalTable: "IRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IRCPESSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    OverallAssessment = table.Column<string>(type: "text", nullable: true),
                    NotablePractice = table.Column<string>(type: "text", nullable: true),
                    PerformanceConcerns = table.Column<string>(type: "text", nullable: true),
                    DevelopmentPriorities = table.Column<string>(type: "text", nullable: true),
                    ConfirmAccuracy = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPESSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRCPESSections_IRCPReviews_IRCPReviewId",
                        column: x => x.IRCPReviewId,
                        principalTable: "IRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IRCPEYDReflections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    Reflection = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPEYDReflections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRCPEYDReflections_IRCPReviews_IRCPReviewId",
                        column: x => x.IRCPReviewId,
                        principalTable: "IRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IRCPPanelReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IRCPReviewId = table.Column<int>(type: "integer", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PanelMembers = table.Column<string>(type: "text", nullable: true),
                    ExtraDocumentation = table.Column<string>(type: "text", nullable: true),
                    RecommendedOutcome = table.Column<string>(type: "text", nullable: true),
                    DetailedReasons = table.Column<string>(type: "text", nullable: true),
                    MitigatingCircumstances = table.Column<string>(type: "text", nullable: true),
                    CompetenciesToDevelop = table.Column<string>(type: "text", nullable: true),
                    RecommendedActions = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IRCPPanelReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IRCPPanelReviews_IRCPReviews_IRCPReviewId",
                        column: x => x.IRCPReviewId,
                        principalTable: "IRCPReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterimReviews_ESUserId",
                table: "InterimReviews",
                column: "ESUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterimReviews_EYDUserId",
                table: "InterimReviews",
                column: "EYDUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IRCPEPAAssessments_EPAId",
                table: "IRCPEPAAssessments",
                column: "EPAId");

            migrationBuilder.CreateIndex(
                name: "IX_IRCPEPAAssessments_InterimReviewId",
                table: "IRCPEPAAssessments",
                column: "InterimReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_IRCPESAssessments_IRCPReviewId",
                table: "IRCPESAssessments",
                column: "IRCPReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_IRCPESSections_IRCPReviewId",
                table: "IRCPESSections",
                column: "IRCPReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_IRCPEYDReflections_IRCPReviewId",
                table: "IRCPEYDReflections",
                column: "IRCPReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IRCPPanelReviews_IRCPReviewId",
                table: "IRCPPanelReviews",
                column: "IRCPReviewId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IRCPEPAAssessments");

            migrationBuilder.DropTable(
                name: "IRCPESAssessments");

            migrationBuilder.DropTable(
                name: "IRCPESSections");

            migrationBuilder.DropTable(
                name: "IRCPEYDReflections");

            migrationBuilder.DropTable(
                name: "IRCPPanelReviews");

            migrationBuilder.DropTable(
                name: "InterimReviews");

            migrationBuilder.DropTable(
                name: "IRCPReviews");
        }
    }
}
