using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class SLE_Implementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SLEId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SLEs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SLEType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    EYDUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Setting = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Audience = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AudienceSetting = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LearningObjectives = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    AssessorUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    ExternalAssessorName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ExternalAssessorEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    ExternalAssessorInstitution = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsInternalAssessor = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalAccessToken = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    InvitationSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAssessmentCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    AssessmentCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentFeedback = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    AssessmentRating = table.Column<int>(type: "integer", nullable: true),
                    ReflectionNotes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ReflectionCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "Draft"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLEs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SLEs_AspNetUsers_AssessorUserId",
                        column: x => x.AssessorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SLEs_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_SLEId",
                table: "EPAMappings",
                column: "SLEId");

            migrationBuilder.CreateIndex(
                name: "IX_SLEs_AssessorUserId",
                table: "SLEs",
                column: "AssessorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SLEs_ExternalAccessToken",
                table: "SLEs",
                column: "ExternalAccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SLEs_EYDUserId",
                table: "SLEs",
                column: "EYDUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SLEs_SLEType",
                table: "SLEs",
                column: "SLEType");

            migrationBuilder.CreateIndex(
                name: "IX_SLEs_Status",
                table: "SLEs",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_SLEs_SLEId",
                table: "EPAMappings",
                column: "SLEId",
                principalTable: "SLEs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_SLEs_SLEId",
                table: "EPAMappings");

            migrationBuilder.DropTable(
                name: "SLEs");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_SLEId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "SLEId",
                table: "EPAMappings");
        }
    }
}
