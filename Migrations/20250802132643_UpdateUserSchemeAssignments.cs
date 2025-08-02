using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSchemeAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchemeId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EYDESAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EYDUserId = table.Column<string>(type: "TEXT", nullable: false),
                    ESUserId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EYDESAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EYDESAssignments_AspNetUsers_ESUserId",
                        column: x => x.ESUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EYDESAssignments_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestingUserId = table.Column<string>(type: "TEXT", nullable: false),
                    TargetEYDUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemporaryAccesses_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TemporaryAccesses_AspNetUsers_RequestingUserId",
                        column: x => x.RequestingUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemporaryAccesses_AspNetUsers_TargetEYDUserId",
                        column: x => x.TargetEYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchemeId",
                table: "AspNetUsers",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_EYDESAssignments_ESUserId",
                table: "EYDESAssignments",
                column: "ESUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EYDESAssignments_EYDUserId",
                table: "EYDESAssignments",
                column: "EYDUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAccesses_ApprovedByUserId",
                table: "TemporaryAccesses",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAccesses_RequestingUserId",
                table: "TemporaryAccesses",
                column: "RequestingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAccesses_TargetEYDUserId",
                table: "TemporaryAccesses",
                column: "TargetEYDUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Schemes_SchemeId",
                table: "AspNetUsers",
                column: "SchemeId",
                principalTable: "Schemes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Schemes_SchemeId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EYDESAssignments");

            migrationBuilder.DropTable(
                name: "TemporaryAccesses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SchemeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchemeId",
                table: "AspNetUsers");
        }
    }
}
