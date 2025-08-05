using System.ComponentModel.DataAnnotations;

namespace EYDGateway.ViewModels
{
    public class ClinicalLogViewModel
    {
        public int Id { get; set; }
        public string EYDUserId { get; set; } = string.Empty;
        public string EYDUserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Month")]
        public string Month { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        public bool IsCompleted { get; set; } = false;

        // General
        [Display(Name = "DNA numbers")]
        public int DNANumbers { get; set; } = 0;

        [Display(Name = "Number of unbooked clinical hours")]
        public int NumberOfUnbookedClinicalHours { get; set; } = 0;

        [Display(Name = "Holidays")]
        public int Holidays { get; set; } = 0;

        [Display(Name = "Sick Days")]
        public int SickDays { get; set; } = 0;

        [Display(Name = "UDAs evidenced on practice software for this specific month")]
        public int UDAsEvidencedOnPracticeSoftware { get; set; } = 0;

        // Clinical Assessment
        [Display(Name = "Adult Examinations")]
        public int AdultExaminations { get; set; } = 0;

        [Display(Name = "Paediatric Examinations (under 16 years of age)")]
        public int PaediatricExaminations { get; set; } = 0;

        [Display(Name = "Adult Radiographs")]
        public int AdultRadiographs { get; set; } = 0;

        [Display(Name = "Paediatric Radiographs (under 16 years of age)")]
        public int PaediatricRadiographs { get; set; } = 0;

        [Display(Name = "Patients with complex Medical Histories")]
        public int PatientsWithComplexMedicalHistories { get; set; } = 0;

        [Display(Name = "6 point periodontal chart")]
        public int SixPointPeriodontalChart { get; set; } = 0;

        // Oral Health Promotion
        [Display(Name = "Diet analysis")]
        public int DietAnalysis { get; set; } = 0;

        [Display(Name = "Fluoride varnish")]
        public int FluorideVarnish { get; set; } = 0;

        // Medical and dental emergencies
        [Display(Name = "Management of medical emergency incident")]
        public int ManagementOfMedicalEmergencyIncident { get; set; } = 0;

        [Display(Name = "Dental trauma")]
        public int DentalTrauma { get; set; } = 0;

        [Display(Name = "Pulp extripation")]
        public int PulpExtripation { get; set; } = 0;

        // Prescribing and therapeutics
        [Display(Name = "Prescribing (antimicrobials)")]
        public int PrescribingAntimicrobials { get; set; } = 0;

        [Display(Name = "IV sedation")]
        public int IVSedation { get; set; } = 0;

        [Display(Name = "Inhalational sedation")]
        public int InhalationalSedation { get; set; } = 0;

        [Display(Name = "General Anaesthesia – Planning and Consent")]
        public int GeneralAnaesthesiaPlanningAndConsent { get; set; } = 0;

        [Display(Name = "General Anaesthesia - Treatment undertaken")]
        public int GeneralAnaesthesiaTreatmentUndertaken { get; set; } = 0;

        // Periodontal Disease
        [Display(Name = "Non-surgical therapy")]
        public int NonSurgicalTherapy { get; set; } = 0;

        // Removal of teeth
        [Display(Name = "Extraction of permanent teeth")]
        public int ExtractionOfPermanentTeeth { get; set; } = 0;

        [Display(Name = "Complex extraction involving sectioning")]
        public int ComplexExtractionInvolvingSectioning { get; set; } = 0;

        [Display(Name = "Suturing")]
        public int Suturing { get; set; } = 0;

        // Management of developing dentition
        [Display(Name = "SS crowns on deciduous teeth")]
        public int SSCrownsOnDeciduousTeeth { get; set; } = 0;

        [Display(Name = "Extraction of deciduous teeth")]
        public int ExtractionOfDeciduousTeeth { get; set; } = 0;

        [Display(Name = "Orthodontic Assessment")]
        public int OrthodonticAssessment { get; set; } = 0;

        // Restoration of teeth
        [Display(Name = "Rubber dam placement")]
        public int RubberDamPlacement { get; set; } = 0;

        [Display(Name = "Amalgam restorations")]
        public int AmalgamRestorations { get; set; } = 0;

        [Display(Name = "Anterior composite restorations")]
        public int AnteriorCompositeRestorations { get; set; } = 0;

        [Display(Name = "Posterior composite restorations")]
        public int PosteriorCompositeRestorations { get; set; } = 0;

        [Display(Name = "GIC")]
        public int GIC { get; set; } = 0;

        [Display(Name = "RCT incisor / canine")]
        public int RCTIncisorCanine { get; set; } = 0;

        [Display(Name = "RCT premolar")]
        public int RCTPremolar { get; set; } = 0;

        [Display(Name = "RCT molar")]
        public int RCTMolar { get; set; } = 0;

        [Display(Name = "Crowns - conventional")]
        public int CrownsConventional { get; set; } = 0;

        [Display(Name = "Onlays")]
        public int Onlays { get; set; } = 0;

        [Display(Name = "Posts")]
        public int Posts { get; set; } = 0;

        [Display(Name = "Bridge - resin retained")]
        public int BridgeResinRetained { get; set; } = 0;

        [Display(Name = "Bridge - conventional")]
        public int BridgeConventional { get; set; } = 0;

        // Replacement of teeth
        [Display(Name = "Acrylic complete dentures")]
        public int AcrylicCompleteDentures { get; set; } = 0;

        [Display(Name = "Acrylic partial dentures")]
        public int AcrylicPartialDentures { get; set; } = 0;

        [Display(Name = "Cobalt-chrome partial dentures")]
        public int CobaltChromePartialDentures { get; set; } = 0;
    }

    public class CreateClinicalLogViewModel : ClinicalLogViewModel
    {
        public CreateClinicalLogViewModel()
        {
            Year = DateTime.Now.Year;
        }
    }

    public class EditClinicalLogViewModel : ClinicalLogViewModel
    {
    }

    public class ClinicalLogTotalsViewModel
    {
        public string EYDUserName { get; set; } = string.Empty;
        public int TotalLogs { get; set; }
        public int CompletedLogs { get; set; }

        // General totals
        public int DNANumbers { get; set; }
        public int NumberOfUnbookedClinicalHours { get; set; }
        public int Holidays { get; set; }
        public int SickDays { get; set; }
        public int UDAsEvidencedOnPracticeSoftware { get; set; }

        // Clinical Assessment totals
        public int AdultExaminations { get; set; }
        public int PaediatricExaminations { get; set; }
        public int AdultRadiographs { get; set; }
        public int PaediatricRadiographs { get; set; }
        public int PatientsWithComplexMedicalHistories { get; set; }
        public int SixPointPeriodontalChart { get; set; }

        // Oral Health Promotion totals
        public int DietAnalysis { get; set; }
        public int FluorideVarnish { get; set; }

        // Medical and dental emergencies totals
        public int ManagementOfMedicalEmergencyIncident { get; set; }
        public int DentalTrauma { get; set; }
        public int PulpExtripation { get; set; }

        // Prescribing and therapeutics totals
        public int PrescribingAntimicrobials { get; set; }
        public int IVSedation { get; set; }
        public int InhalationalSedation { get; set; }
        public int GeneralAnaesthesiaPlanningAndConsent { get; set; }
        public int GeneralAnaesthesiaTreatmentUndertaken { get; set; }

        // Periodontal Disease totals
        public int NonSurgicalTherapy { get; set; }

        // Removal of teeth totals
        public int ExtractionOfPermanentTeeth { get; set; }
        public int ComplexExtractionInvolvingSectioning { get; set; }
        public int Suturing { get; set; }

        // Management of developing dentition totals
        public int SSCrownsOnDeciduousTeeth { get; set; }
        public int ExtractionOfDeciduousTeeth { get; set; }
        public int OrthodonticAssessment { get; set; }

        // Restoration of teeth totals
        public int RubberDamPlacement { get; set; }
        public int AmalgamRestorations { get; set; }
        public int AnteriorCompositeRestorations { get; set; }
        public int PosteriorCompositeRestorations { get; set; }
        public int GIC { get; set; }
        public int RCTIncisorCanine { get; set; }
        public int RCTPremolar { get; set; }
        public int RCTMolar { get; set; }
        public int CrownsConventional { get; set; }
        public int Onlays { get; set; }
        public int Posts { get; set; }
        public int BridgeResinRetained { get; set; }
        public int BridgeConventional { get; set; }

        // Replacement of teeth totals
        public int AcrylicCompleteDentures { get; set; }
        public int AcrylicPartialDentures { get; set; }
        public int CobaltChromePartialDentures { get; set; }
    }
}
