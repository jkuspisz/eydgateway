using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace EYDGateway.Controllers
{
    public class MSFController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MSFController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> MSFFeedback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Invalid questionnaire code");
            }

            var questionnaire = await _context.MSFQuestionnaires
                .Include(q => q.Performer)
                .FirstOrDefaultAsync(q => q.UniqueCode == code && q.IsActive);

            if (questionnaire == null)
            {
                return NotFound("Questionnaire not found or inactive");
            }

            ViewBag.QuestionnaireName = questionnaire.Title;
            ViewBag.PerformerName = questionnaire.Performer?.DisplayName ?? "Unknown";
            ViewBag.QuestionnaireCode = code;

            return View(new SubmitMSFResponseDto { QuestionnaireCode = code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MSFFeedback(SubmitMSFResponseDto model)
        {
            if (!ModelState.IsValid)
            {
                var questionnaire = await _context.MSFQuestionnaires
                    .Include(q => q.Performer)
                    .FirstOrDefaultAsync(q => q.UniqueCode == model.QuestionnaireCode && q.IsActive);

                if (questionnaire != null)
                {
                    ViewBag.QuestionnaireName = questionnaire.Title;
                    ViewBag.PerformerName = questionnaire.Performer?.DisplayName ?? "Unknown";
                    ViewBag.QuestionnaireCode = model.QuestionnaireCode;
                }

                return View(model);
            }

            var questionnaireToUpdate = await _context.MSFQuestionnaires
                .FirstOrDefaultAsync(q => q.UniqueCode == model.QuestionnaireCode && q.IsActive);

            if (questionnaireToUpdate == null)
            {
                return BadRequest("Invalid questionnaire code");
            }

            var response = new MSFResponse
            {
                MSFQuestionnaireId = questionnaireToUpdate.Id,
                
                // Communication scores
                TreatWithCompassionScore = model.TreatWithCompassionScore,
                EnableInformedDecisionsScore = model.EnableInformedDecisionsScore,
                RecogniseCommunicationNeedsScore = model.RecogniseCommunicationNeedsScore,
                ProduceClearCommunicationsScore = model.ProduceClearCommunicationsScore,
                
                // Professionalism scores
                DemonstrateIntegrityScore = model.DemonstrateIntegrityScore,
                WorkWithinScopeScore = model.WorkWithinScopeScore,
                EngageWithDevelopmentScore = model.EngageWithDevelopmentScore,
                KeepPracticeUpToDateScore = model.KeepPracticeUpToDateScore,
                FacilitateLearningScore = model.FacilitateLearningScore,
                InteractWithColleaguesScore = model.InteractWithColleaguesScore,
                PromoteEqualityScore = model.PromoteEqualityScore,
                
                // Management and Leadership scores
                RecogniseImpactOfBehavioursScore = model.RecogniseImpactOfBehavioursScore,
                ManageTimeAndResourcesScore = model.ManageTimeAndResourcesScore,
                WorkAsTeamMemberScore = model.WorkAsTeamMemberScore,
                WorkToStandardsScore = model.WorkToStandardsScore,
                ParticipateInImprovementScore = model.ParticipateInImprovementScore,
                MinimiseWasteScore = model.MinimiseWasteScore,
                
                // Text feedback
                DoesWellComment = model.DoesWellComment,
                CouldImproveComment = model.CouldImproveComment,
                
                SubmittedAt = DateTime.UtcNow
            };

            _context.MSFResponses.Add(response);
            await _context.SaveChangesAsync();

            return View("MSFFeedbackSubmitted");
        }

        public IActionResult QRCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Code is required");
            }

            var feedbackUrl = Url.Action("MSFFeedback", "MSF", 
                new { code = code }, Request.Scheme);

            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(feedbackUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                
                return File(qrCodeBytes, "image/png");
            }
            catch (Exception)
            {
                return BadRequest("Could not generate QR code");
            }
        }
    }
}
