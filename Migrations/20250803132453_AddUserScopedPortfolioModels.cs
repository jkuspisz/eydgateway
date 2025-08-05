using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScopedPortfolioModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicalExperienceLogId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentUploadId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LearningLogId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PortfolioReflectionId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicalExperienceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ExperienceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalSetting = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SupervisorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClinicalActivities = table.Column<string>(type: "text", nullable: false),
                    PatientsSeenDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProceduresPerformed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LearningPoints = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChallengesFaced = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupervisorFeedback = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationHours = table.Column<decimal>(type: "numeric", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalExperienceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalExperienceLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalExperienceLogs_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LearningLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LearningObjectives = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Outcomes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReflectionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationHours = table.Column<decimal>(type: "numeric", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Supervisor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RelatedSLEId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningLogs_SLEs_RelatedSLEId",
                        column: x => x.RelatedSLEId,
                        principalTable: "SLEs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reflections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReflectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    LearningOutcomes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FutureActions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReflectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RelatedSLEId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reflections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reflections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reflections_SLEs_RelatedSLEId",
                        column: x => x.RelatedSLEId,
                        principalTable: "SLEs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RelatedSLEId = table.Column<int>(type: "integer", nullable: true),
                    RelatedReflectionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentUploads_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentUploads_Reflections_RelatedReflectionId",
                        column: x => x.RelatedReflectionId,
                        principalTable: "Reflections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentUploads_SLEs_RelatedSLEId",
                        column: x => x.RelatedSLEId,
                        principalTable: "SLEs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_ClinicalExperienceLogId",
                table: "EPAMappings",
                column: "ClinicalExperienceLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_DocumentUploadId",
                table: "EPAMappings",
                column: "DocumentUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_LearningLogId",
                table: "EPAMappings",
                column: "LearningLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_PortfolioReflectionId",
                table: "EPAMappings",
                column: "PortfolioReflectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalExperienceLogs_UserId",
                table: "ClinicalExperienceLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalExperienceLogs_VerifiedByUserId",
                table: "ClinicalExperienceLogs",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentUploads_RelatedReflectionId",
                table: "DocumentUploads",
                column: "RelatedReflectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentUploads_RelatedSLEId",
                table: "DocumentUploads",
                column: "RelatedSLEId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentUploads_UserId",
                table: "DocumentUploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningLogs_RelatedSLEId",
                table: "LearningLogs",
                column: "RelatedSLEId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningLogs_UserId",
                table: "LearningLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reflections_RelatedSLEId",
                table: "Reflections",
                column: "RelatedSLEId");

            migrationBuilder.CreateIndex(
                name: "IX_Reflections_UserId",
                table: "Reflections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_ClinicalExperienceLogs_ClinicalExperienceLogId",
                table: "EPAMappings",
                column: "ClinicalExperienceLogId",
                principalTable: "ClinicalExperienceLogs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_DocumentUploads_DocumentUploadId",
                table: "EPAMappings",
                column: "DocumentUploadId",
                principalTable: "DocumentUploads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_LearningLogs_LearningLogId",
                table: "EPAMappings",
                column: "LearningLogId",
                principalTable: "LearningLogs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_Reflections_PortfolioReflectionId",
                table: "EPAMappings",
                column: "PortfolioReflectionId",
                principalTable: "Reflections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_ClinicalExperienceLogs_ClinicalExperienceLogId",
                table: "EPAMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_DocumentUploads_DocumentUploadId",
                table: "EPAMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_LearningLogs_LearningLogId",
                table: "EPAMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_Reflections_PortfolioReflectionId",
                table: "EPAMappings");

            migrationBuilder.DropTable(
                name: "ClinicalExperienceLogs");

            migrationBuilder.DropTable(
                name: "DocumentUploads");

            migrationBuilder.DropTable(
                name: "LearningLogs");

            migrationBuilder.DropTable(
                name: "Reflections");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_ClinicalExperienceLogId",
                table: "EPAMappings");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_DocumentUploadId",
                table: "EPAMappings");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_LearningLogId",
                table: "EPAMappings");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_PortfolioReflectionId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "ClinicalExperienceLogId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "DocumentUploadId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "LearningLogId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "PortfolioReflectionId",
                table: "EPAMappings");
        }
    }
}
