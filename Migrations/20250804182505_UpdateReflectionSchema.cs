using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReflectionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FutureActions",
                table: "Reflections");

            migrationBuilder.DropColumn(
                name: "LearningOutcomes",
                table: "Reflections");

            migrationBuilder.DropColumn(
                name: "ReflectionDate",
                table: "Reflections");

            migrationBuilder.DropColumn(
                name: "ReflectionType",
                table: "Reflections");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Reflections",
                newName: "ReasonsForWriting");

            migrationBuilder.AddColumn<string>(
                name: "NextSteps",
                table: "Reflections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhenDidItHappen",
                table: "Reflections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextSteps",
                table: "Reflections");

            migrationBuilder.DropColumn(
                name: "WhenDidItHappen",
                table: "Reflections");

            migrationBuilder.RenameColumn(
                name: "ReasonsForWriting",
                table: "Reflections",
                newName: "Content");

            migrationBuilder.AddColumn<string>(
                name: "FutureActions",
                table: "Reflections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LearningOutcomes",
                table: "Reflections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReflectionDate",
                table: "Reflections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReflectionType",
                table: "Reflections",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
