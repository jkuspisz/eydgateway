using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;

namespace EYDGateway.Controllers
{
    [Authorize]
    public class ClinicalLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClinicalLogController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ClinicalLog
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLogs = await _context.ClinicalLogs
                .Where(cl => cl.EYDUserId == currentUser.Id)
                .OrderByDescending(cl => cl.Year)
                .ThenByDescending(cl => cl.Month)
                .ToListAsync();

            return View(clinicalLogs);
        }

        // GET: ClinicalLog/TotalNumbers
        public async Task<IActionResult> TotalNumbers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLogs = await _context.ClinicalLogs
                .Where(cl => cl.EYDUserId == currentUser.Id)
                .ToListAsync();

            // Calculate totals
            var totals = new ClinicalLogTotalsViewModel
            {
                EYDUserName = currentUser.DisplayName,
                TotalLogs = clinicalLogs.Count,
                CompletedLogs = clinicalLogs.Count(cl => cl.IsCompleted),
                
                // General totals
                DNANumbers = clinicalLogs.Sum(cl => cl.DNANumbers),
                NumberOfUnbookedClinicalHours = clinicalLogs.Sum(cl => cl.NumberOfUnbookedClinicalHours),
                Holidays = clinicalLogs.Sum(cl => cl.Holidays),
                SickDays = clinicalLogs.Sum(cl => cl.SickDays),
                UDAsEvidencedOnPracticeSoftware = clinicalLogs.Sum(cl => cl.UDAsEvidencedOnPracticeSoftware),
                
                // Clinical Assessment totals
                AdultExaminations = clinicalLogs.Sum(cl => cl.AdultExaminations),
                PaediatricExaminations = clinicalLogs.Sum(cl => cl.PaediatricExaminations),
                AdultRadiographs = clinicalLogs.Sum(cl => cl.AdultRadiographs),
                PaediatricRadiographs = clinicalLogs.Sum(cl => cl.PaediatricRadiographs),
                PatientsWithComplexMedicalHistories = clinicalLogs.Sum(cl => cl.PatientsWithComplexMedicalHistories),
                SixPointPeriodontalChart = clinicalLogs.Sum(cl => cl.SixPointPeriodontalChart),
                
                // Oral Health Promotion totals
                DietAnalysis = clinicalLogs.Sum(cl => cl.DietAnalysis),
                FluorideVarnish = clinicalLogs.Sum(cl => cl.FluorideVarnish),
                
                // Medical and dental emergencies totals
                ManagementOfMedicalEmergencyIncident = clinicalLogs.Sum(cl => cl.ManagementOfMedicalEmergencyIncident),
                DentalTrauma = clinicalLogs.Sum(cl => cl.DentalTrauma),
                PulpExtripation = clinicalLogs.Sum(cl => cl.PulpExtripation),
                
                // Prescribing and therapeutics totals
                PrescribingAntimicrobials = clinicalLogs.Sum(cl => cl.PrescribingAntimicrobials),
                IVSedation = clinicalLogs.Sum(cl => cl.IVSedation),
                InhalationalSedation = clinicalLogs.Sum(cl => cl.InhalationalSedation),
                GeneralAnaesthesiaPlanningAndConsent = clinicalLogs.Sum(cl => cl.GeneralAnaesthesiaPlanningAndConsent),
                GeneralAnaesthesiaTreatmentUndertaken = clinicalLogs.Sum(cl => cl.GeneralAnaesthesiaTreatmentUndertaken),
                
                // Periodontal Disease totals
                NonSurgicalTherapy = clinicalLogs.Sum(cl => cl.NonSurgicalTherapy),
                
                // Removal of teeth totals
                ExtractionOfPermanentTeeth = clinicalLogs.Sum(cl => cl.ExtractionOfPermanentTeeth),
                ComplexExtractionInvolvingSectioning = clinicalLogs.Sum(cl => cl.ComplexExtractionInvolvingSectioning),
                Suturing = clinicalLogs.Sum(cl => cl.Suturing),
                
                // Management of developing dentition totals
                SSCrownsOnDeciduousTeeth = clinicalLogs.Sum(cl => cl.SSCrownsOnDeciduousTeeth),
                ExtractionOfDeciduousTeeth = clinicalLogs.Sum(cl => cl.ExtractionOfDeciduousTeeth),
                OrthodonticAssessment = clinicalLogs.Sum(cl => cl.OrthodonticAssessment),
                
                // Restoration of teeth totals
                RubberDamPlacement = clinicalLogs.Sum(cl => cl.RubberDamPlacement),
                AmalgamRestorations = clinicalLogs.Sum(cl => cl.AmalgamRestorations),
                AnteriorCompositeRestorations = clinicalLogs.Sum(cl => cl.AnteriorCompositeRestorations),
                PosteriorCompositeRestorations = clinicalLogs.Sum(cl => cl.PosteriorCompositeRestorations),
                GIC = clinicalLogs.Sum(cl => cl.GIC),
                RCTIncisorCanine = clinicalLogs.Sum(cl => cl.RCTIncisorCanine),
                RCTPremolar = clinicalLogs.Sum(cl => cl.RCTPremolar),
                RCTMolar = clinicalLogs.Sum(cl => cl.RCTMolar),
                CrownsConventional = clinicalLogs.Sum(cl => cl.CrownsConventional),
                Onlays = clinicalLogs.Sum(cl => cl.Onlays),
                Posts = clinicalLogs.Sum(cl => cl.Posts),
                BridgeResinRetained = clinicalLogs.Sum(cl => cl.BridgeResinRetained),
                BridgeConventional = clinicalLogs.Sum(cl => cl.BridgeConventional),
                
                // Replacement of teeth totals
                AcrylicCompleteDentures = clinicalLogs.Sum(cl => cl.AcrylicCompleteDentures),
                AcrylicPartialDentures = clinicalLogs.Sum(cl => cl.AcrylicPartialDentures),
                CobaltChromePartialDentures = clinicalLogs.Sum(cl => cl.CobaltChromePartialDentures)
            };

            return View(totals);
        }

        // GET: ClinicalLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLog = await _context.ClinicalLogs
                .Include(cl => cl.EYDUser)
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog == null)
            {
                return NotFound();
            }

            return View(clinicalLog);
        }

        // GET: ClinicalLog/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var viewModel = new CreateClinicalLogViewModel
            {
                EYDUserId = currentUser.Id,
                EYDUserName = currentUser.UserName ?? "Unknown User"
            };

            return View(viewModel);
        }

        // POST: ClinicalLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ClinicalLog/Create")]
        public async Task<IActionResult> Create(CreateClinicalLogViewModel viewModel)
        {
            Console.WriteLine($"=== POST Create called ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Errors ===");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}");
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"  Error: {subError.ErrorMessage}");
                    }
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    viewModel.EYDUserName = currentUser.UserName ?? "Unknown User";
                }
                return View(viewModel);
            }

            // Check if a log already exists for this month/year/user
            var existingLog = await _context.ClinicalLogs
                .FirstOrDefaultAsync(cl => cl.EYDUserId == viewModel.EYDUserId 
                                        && cl.Month == viewModel.Month 
                                        && cl.Year == viewModel.Year);

            if (existingLog != null)
            {
                ModelState.AddModelError("", "A clinical log already exists for this month and year.");
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    viewModel.EYDUserName = currentUser.UserName ?? "Unknown User";
                }
                return View(viewModel);
            }

            var clinicalLog = new ClinicalLog
            {
                EYDUserId = viewModel.EYDUserId,
                Month = viewModel.Month,
                Year = viewModel.Year,
                IsCompleted = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),

                // General
                DNANumbers = viewModel.DNANumbers,
                NumberOfUnbookedClinicalHours = viewModel.NumberOfUnbookedClinicalHours,
                Holidays = viewModel.Holidays,
                SickDays = viewModel.SickDays,
                UDAsEvidencedOnPracticeSoftware = viewModel.UDAsEvidencedOnPracticeSoftware,

                // Clinical Assessment
                AdultExaminations = viewModel.AdultExaminations,
                PaediatricExaminations = viewModel.PaediatricExaminations,
                AdultRadiographs = viewModel.AdultRadiographs,
                PaediatricRadiographs = viewModel.PaediatricRadiographs,
                PatientsWithComplexMedicalHistories = viewModel.PatientsWithComplexMedicalHistories,
                SixPointPeriodontalChart = viewModel.SixPointPeriodontalChart,

                // Oral Health Promotion
                DietAnalysis = viewModel.DietAnalysis,
                FluorideVarnish = viewModel.FluorideVarnish,

                // Medical and dental emergencies
                ManagementOfMedicalEmergencyIncident = viewModel.ManagementOfMedicalEmergencyIncident,
                DentalTrauma = viewModel.DentalTrauma,
                PulpExtripation = viewModel.PulpExtripation,

                // Prescribing and therapeutics
                PrescribingAntimicrobials = viewModel.PrescribingAntimicrobials,
                IVSedation = viewModel.IVSedation,
                InhalationalSedation = viewModel.InhalationalSedation,
                GeneralAnaesthesiaPlanningAndConsent = viewModel.GeneralAnaesthesiaPlanningAndConsent,
                GeneralAnaesthesiaTreatmentUndertaken = viewModel.GeneralAnaesthesiaTreatmentUndertaken,

                // Periodontal Disease
                NonSurgicalTherapy = viewModel.NonSurgicalTherapy,

                // Removal of teeth
                ExtractionOfPermanentTeeth = viewModel.ExtractionOfPermanentTeeth,
                ComplexExtractionInvolvingSectioning = viewModel.ComplexExtractionInvolvingSectioning,
                Suturing = viewModel.Suturing,

                // Management of developing dentition
                SSCrownsOnDeciduousTeeth = viewModel.SSCrownsOnDeciduousTeeth,
                ExtractionOfDeciduousTeeth = viewModel.ExtractionOfDeciduousTeeth,
                OrthodonticAssessment = viewModel.OrthodonticAssessment,

                // Restoration of teeth
                RubberDamPlacement = viewModel.RubberDamPlacement,
                AmalgamRestorations = viewModel.AmalgamRestorations,
                AnteriorCompositeRestorations = viewModel.AnteriorCompositeRestorations,
                PosteriorCompositeRestorations = viewModel.PosteriorCompositeRestorations,
                GIC = viewModel.GIC,
                RCTIncisorCanine = viewModel.RCTIncisorCanine,
                RCTPremolar = viewModel.RCTPremolar,
                RCTMolar = viewModel.RCTMolar,
                CrownsConventional = viewModel.CrownsConventional,
                Onlays = viewModel.Onlays,
                Posts = viewModel.Posts,
                BridgeResinRetained = viewModel.BridgeResinRetained,
                BridgeConventional = viewModel.BridgeConventional,

                // Replacement of teeth
                AcrylicCompleteDentures = viewModel.AcrylicCompleteDentures,
                AcrylicPartialDentures = viewModel.AcrylicPartialDentures,
                CobaltChromePartialDentures = viewModel.CobaltChromePartialDentures
            };

            try
            {
                _context.Add(clinicalLog);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Clinical log saved successfully with ID: {clinicalLog.Id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving clinical log: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the clinical log.");
                
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    viewModel.EYDUserName = currentUser.UserName ?? "Unknown User";
                }
                return View(viewModel);
            }
        }

        // GET: ClinicalLog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLog = await _context.ClinicalLogs
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog == null)
            {
                return NotFound();
            }

            // Don't allow editing completed logs
            if (clinicalLog.IsCompleted)
            {
                TempData["Error"] = "Cannot edit a completed clinical log.";
                return RedirectToAction(nameof(Details), new { id = clinicalLog.Id });
            }

            var viewModel = new EditClinicalLogViewModel
            {
                Id = clinicalLog.Id,
                EYDUserId = clinicalLog.EYDUserId,
                EYDUserName = currentUser.UserName ?? "Unknown User",
                Month = clinicalLog.Month,
                Year = clinicalLog.Year,
                IsCompleted = clinicalLog.IsCompleted,

                // General
                DNANumbers = clinicalLog.DNANumbers,
                NumberOfUnbookedClinicalHours = clinicalLog.NumberOfUnbookedClinicalHours,
                Holidays = clinicalLog.Holidays,
                SickDays = clinicalLog.SickDays,
                UDAsEvidencedOnPracticeSoftware = clinicalLog.UDAsEvidencedOnPracticeSoftware,

                // Clinical Assessment
                AdultExaminations = clinicalLog.AdultExaminations,
                PaediatricExaminations = clinicalLog.PaediatricExaminations,
                AdultRadiographs = clinicalLog.AdultRadiographs,
                PaediatricRadiographs = clinicalLog.PaediatricRadiographs,
                PatientsWithComplexMedicalHistories = clinicalLog.PatientsWithComplexMedicalHistories,
                SixPointPeriodontalChart = clinicalLog.SixPointPeriodontalChart,

                // Oral Health Promotion
                DietAnalysis = clinicalLog.DietAnalysis,
                FluorideVarnish = clinicalLog.FluorideVarnish,

                // Medical and dental emergencies
                ManagementOfMedicalEmergencyIncident = clinicalLog.ManagementOfMedicalEmergencyIncident,
                DentalTrauma = clinicalLog.DentalTrauma,
                PulpExtripation = clinicalLog.PulpExtripation,

                // Prescribing and therapeutics
                PrescribingAntimicrobials = clinicalLog.PrescribingAntimicrobials,
                IVSedation = clinicalLog.IVSedation,
                InhalationalSedation = clinicalLog.InhalationalSedation,
                GeneralAnaesthesiaPlanningAndConsent = clinicalLog.GeneralAnaesthesiaPlanningAndConsent,
                GeneralAnaesthesiaTreatmentUndertaken = clinicalLog.GeneralAnaesthesiaTreatmentUndertaken,

                // Periodontal Disease
                NonSurgicalTherapy = clinicalLog.NonSurgicalTherapy,

                // Removal of teeth
                ExtractionOfPermanentTeeth = clinicalLog.ExtractionOfPermanentTeeth,
                ComplexExtractionInvolvingSectioning = clinicalLog.ComplexExtractionInvolvingSectioning,
                Suturing = clinicalLog.Suturing,

                // Management of developing dentition
                SSCrownsOnDeciduousTeeth = clinicalLog.SSCrownsOnDeciduousTeeth,
                ExtractionOfDeciduousTeeth = clinicalLog.ExtractionOfDeciduousTeeth,
                OrthodonticAssessment = clinicalLog.OrthodonticAssessment,

                // Restoration of teeth
                RubberDamPlacement = clinicalLog.RubberDamPlacement,
                AmalgamRestorations = clinicalLog.AmalgamRestorations,
                AnteriorCompositeRestorations = clinicalLog.AnteriorCompositeRestorations,
                PosteriorCompositeRestorations = clinicalLog.PosteriorCompositeRestorations,
                GIC = clinicalLog.GIC,
                RCTIncisorCanine = clinicalLog.RCTIncisorCanine,
                RCTPremolar = clinicalLog.RCTPremolar,
                RCTMolar = clinicalLog.RCTMolar,
                CrownsConventional = clinicalLog.CrownsConventional,
                Onlays = clinicalLog.Onlays,
                Posts = clinicalLog.Posts,
                BridgeResinRetained = clinicalLog.BridgeResinRetained,
                BridgeConventional = clinicalLog.BridgeConventional,

                // Replacement of teeth
                AcrylicCompleteDentures = clinicalLog.AcrylicCompleteDentures,
                AcrylicPartialDentures = clinicalLog.AcrylicPartialDentures,
                CobaltChromePartialDentures = clinicalLog.CobaltChromePartialDentures
            };

            return View(viewModel);
        }

        // POST: ClinicalLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditClinicalLogViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                viewModel.EYDUserName = currentUser.UserName ?? "Unknown User";
                return View(viewModel);
            }

            var clinicalLog = await _context.ClinicalLogs
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog == null)
            {
                return NotFound();
            }

            // Don't allow editing completed logs
            if (clinicalLog.IsCompleted)
            {
                TempData["Error"] = "Cannot edit a completed clinical log.";
                return RedirectToAction(nameof(Details), new { id = clinicalLog.Id });
            }

            try
            {
                // Update all fields
                clinicalLog.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                // General
                clinicalLog.DNANumbers = viewModel.DNANumbers;
                clinicalLog.NumberOfUnbookedClinicalHours = viewModel.NumberOfUnbookedClinicalHours;
                clinicalLog.Holidays = viewModel.Holidays;
                clinicalLog.SickDays = viewModel.SickDays;
                clinicalLog.UDAsEvidencedOnPracticeSoftware = viewModel.UDAsEvidencedOnPracticeSoftware;

                // Clinical Assessment
                clinicalLog.AdultExaminations = viewModel.AdultExaminations;
                clinicalLog.PaediatricExaminations = viewModel.PaediatricExaminations;
                clinicalLog.AdultRadiographs = viewModel.AdultRadiographs;
                clinicalLog.PaediatricRadiographs = viewModel.PaediatricRadiographs;
                clinicalLog.PatientsWithComplexMedicalHistories = viewModel.PatientsWithComplexMedicalHistories;
                clinicalLog.SixPointPeriodontalChart = viewModel.SixPointPeriodontalChart;

                // Oral Health Promotion
                clinicalLog.DietAnalysis = viewModel.DietAnalysis;
                clinicalLog.FluorideVarnish = viewModel.FluorideVarnish;

                // Medical and dental emergencies
                clinicalLog.ManagementOfMedicalEmergencyIncident = viewModel.ManagementOfMedicalEmergencyIncident;
                clinicalLog.DentalTrauma = viewModel.DentalTrauma;
                clinicalLog.PulpExtripation = viewModel.PulpExtripation;

                // Prescribing and therapeutics
                clinicalLog.PrescribingAntimicrobials = viewModel.PrescribingAntimicrobials;
                clinicalLog.IVSedation = viewModel.IVSedation;
                clinicalLog.InhalationalSedation = viewModel.InhalationalSedation;
                clinicalLog.GeneralAnaesthesiaPlanningAndConsent = viewModel.GeneralAnaesthesiaPlanningAndConsent;
                clinicalLog.GeneralAnaesthesiaTreatmentUndertaken = viewModel.GeneralAnaesthesiaTreatmentUndertaken;

                // Periodontal Disease
                clinicalLog.NonSurgicalTherapy = viewModel.NonSurgicalTherapy;

                // Removal of teeth
                clinicalLog.ExtractionOfPermanentTeeth = viewModel.ExtractionOfPermanentTeeth;
                clinicalLog.ComplexExtractionInvolvingSectioning = viewModel.ComplexExtractionInvolvingSectioning;
                clinicalLog.Suturing = viewModel.Suturing;

                // Management of developing dentition
                clinicalLog.SSCrownsOnDeciduousTeeth = viewModel.SSCrownsOnDeciduousTeeth;
                clinicalLog.ExtractionOfDeciduousTeeth = viewModel.ExtractionOfDeciduousTeeth;
                clinicalLog.OrthodonticAssessment = viewModel.OrthodonticAssessment;

                // Restoration of teeth
                clinicalLog.RubberDamPlacement = viewModel.RubberDamPlacement;
                clinicalLog.AmalgamRestorations = viewModel.AmalgamRestorations;
                clinicalLog.AnteriorCompositeRestorations = viewModel.AnteriorCompositeRestorations;
                clinicalLog.PosteriorCompositeRestorations = viewModel.PosteriorCompositeRestorations;
                clinicalLog.GIC = viewModel.GIC;
                clinicalLog.RCTIncisorCanine = viewModel.RCTIncisorCanine;
                clinicalLog.RCTPremolar = viewModel.RCTPremolar;
                clinicalLog.RCTMolar = viewModel.RCTMolar;
                clinicalLog.CrownsConventional = viewModel.CrownsConventional;
                clinicalLog.Onlays = viewModel.Onlays;
                clinicalLog.Posts = viewModel.Posts;
                clinicalLog.BridgeResinRetained = viewModel.BridgeResinRetained;
                clinicalLog.BridgeConventional = viewModel.BridgeConventional;

                // Replacement of teeth
                clinicalLog.AcrylicCompleteDentures = viewModel.AcrylicCompleteDentures;
                clinicalLog.AcrylicPartialDentures = viewModel.AcrylicPartialDentures;
                clinicalLog.CobaltChromePartialDentures = viewModel.CobaltChromePartialDentures;

                _context.Update(clinicalLog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicalLogExists(clinicalLog.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating clinical log: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the clinical log.");
                viewModel.EYDUserName = currentUser.UserName ?? "Unknown User";
                return View(viewModel);
            }
        }

        // POST: ClinicalLog/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLog = await _context.ClinicalLogs
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog == null)
            {
                return NotFound();
            }

            clinicalLog.IsCompleted = true;
            clinicalLog.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

            _context.Update(clinicalLog);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Clinical log for {clinicalLog.Month} {clinicalLog.Year} has been completed.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ClinicalLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLog = await _context.ClinicalLogs
                .Include(cl => cl.EYDUser)
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog == null)
            {
                return NotFound();
            }

            return View(clinicalLog);
        }

        // POST: ClinicalLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var clinicalLog = await _context.ClinicalLogs
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.EYDUserId == currentUser.Id);

            if (clinicalLog != null)
            {
                _context.ClinicalLogs.Remove(clinicalLog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClinicalLogExists(int id)
        {
            return _context.ClinicalLogs.Any(e => e.Id == id);
        }
    }
}
