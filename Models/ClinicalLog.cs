using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EYDGateway.Models
{
    public class ClinicalLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EYDUserId { get; set; } = string.Empty;

        [ForeignKey("EYDUserId")]
        public virtual ApplicationUser? EYDUser { get; set; }

        [Required]
        [StringLength(20)]
        public string Month { get; set; } = string.Empty; // e.g., "September", "October"

        [Required]
        public int Year { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // General (5 fields)
        public int DNANumbers { get; set; } = 0;
        public int NumberOfUnbookedClinicalHours { get; set; } = 0;
        public int Holidays { get; set; } = 0;
        public int SickDays { get; set; } = 0;
        public int UDAsEvidencedOnPracticeSoftware { get; set; } = 0;

        // Clinical Assessment (6 fields)
        public int AdultExaminations { get; set; } = 0;
        public int PaediatricExaminations { get; set; } = 0;
        public int AdultRadiographs { get; set; } = 0;
        public int PaediatricRadiographs { get; set; } = 0;
        public int PatientsWithComplexMedicalHistories { get; set; } = 0;
        public int SixPointPeriodontalChart { get; set; } = 0;

        // Oral Health Promotion (2 fields)
        public int DietAnalysis { get; set; } = 0;
        public int FluorideVarnish { get; set; } = 0;

        // Medical and dental emergencies (3 fields)
        public int ManagementOfMedicalEmergencyIncident { get; set; } = 0;
        public int DentalTrauma { get; set; } = 0;
        public int PulpExtripation { get; set; } = 0;

        // Prescribing and therapeutics (5 fields)
        public int PrescribingAntimicrobials { get; set; } = 0;
        public int IVSedation { get; set; } = 0;
        public int InhalationalSedation { get; set; } = 0;
        public int GeneralAnaesthesiaPlanningAndConsent { get; set; } = 0;
        public int GeneralAnaesthesiaTreatmentUndertaken { get; set; } = 0;

        // Periodontal Disease (1 field)
        public int NonSurgicalTherapy { get; set; } = 0;

        // Removal of teeth (3 fields)
        public int ExtractionOfPermanentTeeth { get; set; } = 0;
        public int ComplexExtractionInvolvingSectioning { get; set; } = 0;
        public int Suturing { get; set; } = 0;

        // Management of developing dentition (3 fields)
        public int SSCrownsOnDeciduousTeeth { get; set; } = 0;
        public int ExtractionOfDeciduousTeeth { get; set; } = 0;
        public int OrthodonticAssessment { get; set; } = 0;

        // Restoration of teeth (13 fields)
        public int RubberDamPlacement { get; set; } = 0;
        public int AmalgamRestorations { get; set; } = 0;
        public int AnteriorCompositeRestorations { get; set; } = 0;
        public int PosteriorCompositeRestorations { get; set; } = 0;
        public int GIC { get; set; } = 0;
        public int RCTIncisorCanine { get; set; } = 0;
        public int RCTPremolar { get; set; } = 0;
        public int RCTMolar { get; set; } = 0;
        public int CrownsConventional { get; set; } = 0;
        public int Onlays { get; set; } = 0;
        public int Posts { get; set; } = 0;
        public int BridgeResinRetained { get; set; } = 0;
        public int BridgeConventional { get; set; } = 0;

        // Replacement of teeth (3 fields)
        public int AcrylicCompleteDentures { get; set; } = 0;
        public int AcrylicPartialDentures { get; set; } = 0;
        public int CobaltChromePartialDentures { get; set; } = 0;
    }
}
