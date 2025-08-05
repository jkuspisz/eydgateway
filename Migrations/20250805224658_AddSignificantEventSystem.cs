using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddSignificantEventSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SignificantEventId",
                table: "EPAMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SignificantEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    AccountOfExperience = table.Column<string>(type: "text", nullable: false),
                    AnalysisOfSituation = table.Column<string>(type: "text", nullable: false),
                    ReflectionOnEvent = table.Column<string>(type: "text", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    ESSignedOff = table.Column<bool>(type: "boolean", nullable: false),
                    ESSignedOffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ESUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    TPDSignedOff = table.Column<bool>(type: "boolean", nullable: false),
                    TPDSignedOffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TPDUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignificantEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignificantEvents_AspNetUsers_ESUserId",
                        column: x => x.ESUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SignificantEvents_AspNetUsers_TPDUserId",
                        column: x => x.TPDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SignificantEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EPAMappings_SignificantEventId",
                table: "EPAMappings",
                column: "SignificantEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SignificantEvents_ESSignedOff",
                table: "SignificantEvents",
                column: "ESSignedOff");

            migrationBuilder.CreateIndex(
                name: "IX_SignificantEvents_ESUserId",
                table: "SignificantEvents",
                column: "ESUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SignificantEvents_TPDSignedOff",
                table: "SignificantEvents",
                column: "TPDSignedOff");

            migrationBuilder.CreateIndex(
                name: "IX_SignificantEvents_TPDUserId",
                table: "SignificantEvents",
                column: "TPDUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SignificantEvents_UserId",
                table: "SignificantEvents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAMappings_SignificantEvents_SignificantEventId",
                table: "EPAMappings",
                column: "SignificantEventId",
                principalTable: "SignificantEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAMappings_SignificantEvents_SignificantEventId",
                table: "EPAMappings");

            migrationBuilder.DropTable(
                name: "SignificantEvents");

            migrationBuilder.DropIndex(
                name: "IX_EPAMappings_SignificantEventId",
                table: "EPAMappings");

            migrationBuilder.DropColumn(
                name: "SignificantEventId",
                table: "EPAMappings");
        }
    }
}
