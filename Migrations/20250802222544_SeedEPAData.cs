using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class SeedEPAData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = new DateTime(2025, 8, 2, 22, 25, 0, DateTimeKind.Utc);
            
            migrationBuilder.InsertData(
                table: "EPAs",
                columns: new[] { "Code", "Title", "Description", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { "EPA1", "Assessing and managing new patients", "Core competency for initial patient assessment and treatment planning", true, now },
                    { "EPA2A", "Providing routine dental care: periodontal and restorative", "Core competency for standard periodontal and restorative dental procedures", true, now },
                    { "EPA2B", "Providing routine dental care: removal and replacement of teeth", "Core competency for extractions and prosthetic tooth replacement", true, now },
                    { "EPA3", "Assessing and managing children and young people", "Core competency for pediatric and adolescent dental care", true, now },
                    { "EPA4", "Providing emergency care", "Core competency for urgent and emergency dental treatment", true, now },
                    { "EPA5", "Assessing and managing patients with complex needs", "Core competency for advanced and complex patient care", true, now },
                    { "EPA6", "Promoting oral health in the population", "Core competency for public health and preventive dentistry", true, now },
                    { "EPA7", "Managing the service", "Core competency for practice management and leadership", true, now },
                    { "EPA8", "Improving the quality of dental services", "Core competency for quality improvement and audit", true, now },
                    { "EPA9", "Developing self and others", "Core competency for professional development and teaching", true, now }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EPAs",
                keyColumn: "Code",
                keyValues: new object[] { "EPA1", "EPA2A", "EPA2B", "EPA3", "EPA4", "EPA5", "EPA6", "EPA7", "EPA8", "EPA9" });
        }
    }
}
