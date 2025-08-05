using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EYDGateway.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EYDUserId = table.Column<string>(type: "text", nullable: false),
                    Month = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DNANumbers = table.Column<int>(type: "integer", nullable: false),
                    NumberOfUnbookedClinicalHours = table.Column<int>(type: "integer", nullable: false),
                    Holidays = table.Column<int>(type: "integer", nullable: false),
                    SickDays = table.Column<int>(type: "integer", nullable: false),
                    UDAsEvidencedOnPracticeSoftware = table.Column<int>(type: "integer", nullable: false),
                    AdultExaminations = table.Column<int>(type: "integer", nullable: false),
                    PaediatricExaminations = table.Column<int>(type: "integer", nullable: false),
                    AdultRadiographs = table.Column<int>(type: "integer", nullable: false),
                    PaediatricRadiographs = table.Column<int>(type: "integer", nullable: false),
                    PatientsWithComplexMedicalHistories = table.Column<int>(type: "integer", nullable: false),
                    SixPointPeriodontalChart = table.Column<int>(type: "integer", nullable: false),
                    DietAnalysis = table.Column<int>(type: "integer", nullable: false),
                    FluorideVarnish = table.Column<int>(type: "integer", nullable: false),
                    ManagementOfMedicalEmergencyIncident = table.Column<int>(type: "integer", nullable: false),
                    DentalTrauma = table.Column<int>(type: "integer", nullable: false),
                    PulpExtripation = table.Column<int>(type: "integer", nullable: false),
                    PrescribingAntimicrobials = table.Column<int>(type: "integer", nullable: false),
                    IVSedation = table.Column<int>(type: "integer", nullable: false),
                    InhalationalSedation = table.Column<int>(type: "integer", nullable: false),
                    GeneralAnaesthesiaPlanningAndConsent = table.Column<int>(type: "integer", nullable: false),
                    GeneralAnaesthesiaTreatmentUndertaken = table.Column<int>(type: "integer", nullable: false),
                    NonSurgicalTherapy = table.Column<int>(type: "integer", nullable: false),
                    ExtractionOfPermanentTeeth = table.Column<int>(type: "integer", nullable: false),
                    ComplexExtractionInvolvingSectioning = table.Column<int>(type: "integer", nullable: false),
                    Suturing = table.Column<int>(type: "integer", nullable: false),
                    SSCrownsOnDeciduousTeeth = table.Column<int>(type: "integer", nullable: false),
                    ExtractionOfDeciduousTeeth = table.Column<int>(type: "integer", nullable: false),
                    OrthodonticAssessment = table.Column<int>(type: "integer", nullable: false),
                    RubberDamPlacement = table.Column<int>(type: "integer", nullable: false),
                    AmalgamRestorations = table.Column<int>(type: "integer", nullable: false),
                    AnteriorCompositeRestorations = table.Column<int>(type: "integer", nullable: false),
                    PosteriorCompositeRestorations = table.Column<int>(type: "integer", nullable: false),
                    GIC = table.Column<int>(type: "integer", nullable: false),
                    RCTIncisorCanine = table.Column<int>(type: "integer", nullable: false),
                    RCTPremolar = table.Column<int>(type: "integer", nullable: false),
                    RCTMolar = table.Column<int>(type: "integer", nullable: false),
                    CrownsConventional = table.Column<int>(type: "integer", nullable: false),
                    Onlays = table.Column<int>(type: "integer", nullable: false),
                    Posts = table.Column<int>(type: "integer", nullable: false),
                    BridgeResinRetained = table.Column<int>(type: "integer", nullable: false),
                    BridgeConventional = table.Column<int>(type: "integer", nullable: false),
                    AcrylicCompleteDentures = table.Column<int>(type: "integer", nullable: false),
                    AcrylicPartialDentures = table.Column<int>(type: "integer", nullable: false),
                    CobaltChromePartialDentures = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalLogs_AspNetUsers_EYDUserId",
                        column: x => x.EYDUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalLogs_EYDUserId",
                table: "ClinicalLogs",
                column: "EYDUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalLogs");
        }
    }
}
