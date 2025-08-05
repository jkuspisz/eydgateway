using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddESInductionFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ESInductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "varchar(450)", nullable: false),
                    ESUserId = table.Column<string>(type: "varchar(450)", nullable: false),
                    HasReadTransitionDocumentAndAgreedPDP = table.Column<bool>(type: "boolean", nullable: false),
                    MeetingNotesAndComments = table.Column<string>(type: "text", nullable: false),
                    PlacementDescription = table.Column<string>(type: "text", nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESInductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ESInductions_AspNetUsers_ESUserId",
                        column: x => x.ESUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ESInductions_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ESInductions_ESUserId",
                table: "ESInductions",
                column: "ESUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ESInductions_EYDUserId",
                table: "ESInductions",
                column: "EYDUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ESInductions");
        }
    }
}
