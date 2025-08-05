using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSLEFromReflection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reflections_SLEs_RelatedSLEId",
                table: "Reflections");

            migrationBuilder.DropIndex(
                name: "IX_Reflections_RelatedSLEId",
                table: "Reflections");

            migrationBuilder.DropColumn(
                name: "RelatedSLEId",
                table: "Reflections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedSLEId",
                table: "Reflections",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reflections_RelatedSLEId",
                table: "Reflections",
                column: "RelatedSLEId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reflections_SLEs_RelatedSLEId",
                table: "Reflections",
                column: "RelatedSLEId",
                principalTable: "SLEs",
                principalColumn: "Id");
        }
    }
}
