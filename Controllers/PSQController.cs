using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EYDGateway.Data;
using EYDGateway.Models;
using EYDGateway.ViewModels;
using QRCoder;

namespace EYDGateway.Controllers
{
    public class PSQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PSQController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> PSQFeedback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Invalid PSQ code");
            }

            var questionnaire = await _context.PSQQuestionnaires
                .Include(q => q.Performer)
                .FirstOrDefaultAsync(q => q.UniqueCode == code && q.IsActive);

            if (questionnaire == null)
            {
                return NotFound("PSQ questionnaire not found or is no longer active");
            }

            var model = new SubmitPSQResponseDto
            {
                QuestionnaireCode = code
            };

            ViewBag.PerformerName = questionnaire.Performer?.DisplayName ?? "Unknown Practitioner";
            ViewBag.QuestionnaireName = questionnaire.Title;

            return View("PSQFeedback", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PSQFeedback(SubmitPSQResponseDto model)
        {
            if (!ModelState.IsValid)
            {
                var questionnaire = await _context.PSQQuestionnaires
                    .Include(q => q.Performer)
                    .FirstOrDefaultAsync(q => q.UniqueCode == model.QuestionnaireCode && q.IsActive);

                if (questionnaire != null)
                {
                    ViewBag.PerformerName = questionnaire.Performer?.DisplayName ?? "Unknown Practitioner";
                    ViewBag.QuestionnaireName = questionnaire.Title;
                }

                return View("PSQFeedback", model);
            }

            var psqQuestionnaire = await _context.PSQQuestionnaires
                .FirstOrDefaultAsync(q => q.UniqueCode == model.QuestionnaireCode && q.IsActive);

            if (psqQuestionnaire == null)
            {
                return NotFound("PSQ questionnaire not found or is no longer active");
            }

            // Create the response
            var response = new PSQResponse
            {
                PSQQuestionnaireId = psqQuestionnaire.Id,
                SubmittedAt = DateTime.UtcNow,
                
                // All 12 patient satisfaction scores
                PutMeAtEaseScore = model.PutMeAtEaseScore,
                TreatedWithDignityScore = model.TreatedWithDignityScore,
                ListenedToConcernsScore = model.ListenedToConcernsScore,
                ExplainedTreatmentOptionsScore = model.ExplainedTreatmentOptionsScore,
                InvolvedInDecisionsScore = model.InvolvedInDecisionsScore,
                InvolvedFamilyScore = model.InvolvedFamilyScore,
                TailoredApproachScore = model.TailoredApproachScore,
                ExplainedNextStepsScore = model.ExplainedNextStepsScore,
                ProvidedGuidanceScore = model.ProvidedGuidanceScore,
                AllocatedTimeScore = model.AllocatedTimeScore,
                WorkedWithTeamScore = model.WorkedWithTeamScore,
                CanTrustDentistScore = model.CanTrustDentistScore,
                
                // Open-ended feedback
                DoesWellComment = model.DoesWellComment?.Trim(),
                CouldImproveComment = model.CouldImproveComment?.Trim()
            };

            _context.PSQResponses.Add(response);
            await _context.SaveChangesAsync();

            return View("PSQFeedbackSubmitted");
        }

        public IActionResult QRCode(string code)
        {
            var feedbackUrl = Url.Action("PSQFeedback", "PSQ", new { code = code }, Request.Scheme);
            
            if (string.IsNullOrEmpty(feedbackUrl))
            {
                return BadRequest("Could not generate feedback URL");
            }

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
