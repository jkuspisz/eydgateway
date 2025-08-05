using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddProtectedLearningTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProtectedLearningTimeId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProtectedLearningTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Format = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LengthOfPLT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhenAndWhoLed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BriefOutlineOfLearning = table.Column<string>(type: "text", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectedLearningTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtectedLearningTimes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_ProtectedLearningTimeId",
                table: "EPAMappings",
                column: "ProtectedLearningTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectedLearningTimes_UserId",
                table: "ProtectedLearningTimes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_ProtectedLearningTimes_ProtectedLearningTimeId",
                table: "EPAMappings",
                column: "ProtectedLearningTimeId",
                principalTable: "ProtectedLearningTimes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_ProtectedLearningTimes_ProtectedLearningTimeId",
                table: "EPAMappings");

            migrationBuilder.DropTable(
                name: "ProtectedLearningTimes");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_ProtectedLearningTimeId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "ProtectedLearningTimeId",
                table: "EPAMappings");
        }
    }
}
