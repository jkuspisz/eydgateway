using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddAdHocESReportSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdHocESReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "text", nullable: false),
                    ESUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ESCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EYDCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsESCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsEYDCompleted = table.Column<bool>(type: "boolean", nullable: false),
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
                    EYDActionPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdHocESReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdHocESReports_AspNetUsers_ESUserId",
                        column: x => x.ESUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdHocESReports_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdHocESReportEPAAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdHocESReportId = table.Column<int>(type: "integer", nullable: false),
                    EPAId = table.Column<int>(type: "integer", nullable: false),
                    ProgressLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdHocESReportEPAAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdHocESReportEPAAssessments_AdHocESReports_AdHocESReportId",
                        column: x => x.AdHocESReportId,
                        principalTable: "AdHocESReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdHocESReportEPAAssessments_EPAs_EPAId",
                        column: x => x.EPAId,
                        principalTable: "EPAs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdHocESReportEPAAssessments_AdHocESReportId",
                table: "AdHocESReportEPAAssessments",
                column: "AdHocESReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AdHocESReportEPAAssessments_EPAId",
                table: "AdHocESReportEPAAssessments",
                column: "EPAId");

            migrationBuilder.CreateIndex(
                name: "IX_AdHocESReports_ESUserId",
                table: "AdHocESReports",
                column: "ESUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdHocESReports_EYDUserId",
                table: "AdHocESReports",
                column: "EYDUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdHocESReportEPAAssessments");

            migrationBuilder.DropTable(
                name: "AdHocESReports");
        }
    }
}
