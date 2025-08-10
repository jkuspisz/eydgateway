using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EYDGateway.Data;
using EYDGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace EYDGateway.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly ApplicationDbContext _context;

        public AIAnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AIAnalysisResult> AnalyzePortfolioAsync(string eydUserId, string mode = "overall")
        {
            var sb = new StringBuilder();

            // Basic identity
            var user = await _context.Users.Include(u => u.Scheme).FirstOrDefaultAsync(u => u.Id == eydUserId);
            var name = user?.DisplayName ?? user?.UserName ?? eydUserId;

            // SLE metrics
            var sleTypes = new[] { "CBD", "DOPS", "MiniCEX", "DOPSSim", "DtCT", "DENTL" };
            var sleCounts = new System.Collections.Generic.Dictionary<string, (int total, int completed)>();
            foreach (var t in sleTypes)
            {
                var total = await _context.SLEs.CountAsync(s => s.EYDUserId == eydUserId && s.SLEType == t);
                var completed = await _context.SLEs.CountAsync(s => s.EYDUserId == eydUserId && s.SLEType == t && s.IsAssessmentCompleted && s.ReflectionCompletedAt != null);
                sleCounts[t] = (total, completed);
            }

            // PLT
            var pltTotal = await _context.ProtectedLearningTimes.CountAsync(p => p.UserId == eydUserId);
            var pltComplete = await _context.ProtectedLearningTimes.CountAsync(p => p.UserId == eydUserId && p.IsLocked);

            // Reflections
            var reflectionsTotal = await _context.Reflections.CountAsync(r => r.UserId == eydUserId);
            var reflectionsComplete = await _context.Reflections.CountAsync(r => r.UserId == eydUserId && r.IsLocked);

            // Learning needs
            var lnTotal = await _context.LearningNeeds.CountAsync(l => l.UserId == eydUserId);
            var lnCompleted = await _context.LearningNeeds.CountAsync(l => l.UserId == eydUserId && l.Status == Models.LearningNeedStatus.Completed);

            // Recent activity dates
            var latestSLE = await _context.SLEs.Where(s => s.EYDUserId == eydUserId)
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => s.UpdatedAt)
                .FirstOrDefaultAsync();
            var latestPLT = await _context.ProtectedLearningTimes.Where(p => p.UserId == eydUserId)
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => p.UpdatedAt)
                .FirstOrDefaultAsync();
            var latestRefl = await _context.Reflections.Where(r => r.UserId == eydUserId)
                .OrderByDescending(r => r.UpdatedAt)
                .Select(r => r.UpdatedAt)
                .FirstOrDefaultAsync();
            var latestLN = await _context.LearningNeeds.Where(l => l.UserId == eydUserId)
                .OrderByDescending(l => l.UpdatedAt)
                .Select(l => l.UpdatedAt)
                .FirstOrDefaultAsync();

            // ES feedback snapshot (Ad-hoc reports)
            var latestES = await _context.AdHocESReports.Where(a => a.EYDUserId == eydUserId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            // Collect narrative content for lightweight NLP
            var reflections = await _context.Reflections
                .Where(r => r.UserId == eydUserId)
                .Select(r => new { r.Title, r.ReasonsForWriting, r.NextSteps, r.WhenDidItHappen, r.UpdatedAt })
                .ToListAsync();
            var plts = await _context.ProtectedLearningTimes
                .Where(p => p.UserId == eydUserId)
                .Select(p => new { p.Title, p.BriefOutlineOfLearning, p.WhenAndWhoLed, p.Format, p.UpdatedAt })
                .ToListAsync();
            var learningNeeds = await _context.LearningNeeds
                .Where(l => l.UserId == eydUserId)
                .Select(l => new { l.Name, l.LearningObjectives, l.HowToAddress, l.ReflectionOnMeeting, l.UpdatedAt, l.Status })
                .ToListAsync();
            var sles = await _context.SLEs
                .Where(s => s.EYDUserId == eydUserId)
                .Select(s => new { s.SLEType, s.Title, s.Description, s.BehaviourFeedback, s.AgreedAction, s.ReflectionNotes, s.UpdatedAt })
                .ToListAsync();
            var esNotes = await _context.AdHocESReports
                .Where(a => a.EYDUserId == eydUserId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .Select(a => new { a.ESOverallAssessment, a.ESStrengths, a.ESAreasForDevelopment, a.ESRecommendations, a.ESAdditionalComments, a.ESProgressSinceLastReview, a.ESClinicalPerformance, a.ESProfessionalBehavior, a.EYDReflectionComments, a.EYDLearningGoals, a.EYDActionPlan })
                .ToListAsync();

            var corpus = new StringBuilder();
            void AppendIf(string? v)
            {
                if (!string.IsNullOrWhiteSpace(v)) corpus.Append(' ').Append(v);
            }
            foreach (var r in reflections)
            {
                AppendIf(r.Title); AppendIf(r.WhenDidItHappen); AppendIf(r.ReasonsForWriting); AppendIf(r.NextSteps);
            }
            foreach (var p in plts)
            {
                AppendIf(p.Title); AppendIf(p.BriefOutlineOfLearning); AppendIf(p.WhenAndWhoLed); AppendIf(p.Format);
            }
            foreach (var l in learningNeeds)
            {
                AppendIf(l.Name); AppendIf(l.LearningObjectives); AppendIf(l.HowToAddress); AppendIf(l.ReflectionOnMeeting);
            }
            foreach (var s in sles)
            {
                AppendIf(s.Title); AppendIf(s.Description); AppendIf(s.BehaviourFeedback); AppendIf(s.AgreedAction); AppendIf(s.ReflectionNotes);
            }
            foreach (var e in esNotes)
            {
                AppendIf(e.ESOverallAssessment); AppendIf(e.ESStrengths); AppendIf(e.ESAreasForDevelopment); AppendIf(e.ESRecommendations);
                AppendIf(e.ESAdditionalComments); AppendIf(e.ESProgressSinceLastReview); AppendIf(e.ESClinicalPerformance); AppendIf(e.ESProfessionalBehavior);
                AppendIf(e.EYDReflectionComments); AppendIf(e.EYDLearningGoals); AppendIf(e.EYDActionPlan);
            }

            var topKeywords = ExtractTopKeywords(corpus.ToString(), 5);

            // EPA coverage (counts per EPA across all mapped entities for this user)
            var epaCounts = await _context.EPAMappings
                .Where(m => m.UserId == eydUserId)
                .GroupBy(m => m.EPAId)
                .Select(g => new { EPAId = g.Key, Count = g.Count() })
                .ToListAsync();
            var epaIds = epaCounts.Select(e => e.EPAId).ToList();
            var epaDict = await _context.EPAs
                .Where(e => e.IsActive)
                .Select(e => new { e.Id, e.Code, e.Title })
                .ToDictionaryAsync(e => e.Id, e => (e.Code, e.Title));
            var topEpas = epaCounts.OrderByDescending(e => e.Count).Take(3)
                .Select(e => epaDict.TryGetValue(e.EPAId, out var meta) ? $"{meta.Code} ({e.Count})" : $"EPA {e.EPAId} ({e.Count})");
            var gapEpas = epaDict.Keys.Where(id => !epaIds.Contains(id)).Take(3)
                .Select(id => epaDict[id].Code);

            // Recent activity window (last 30 days)
            var since = DateTime.UtcNow.AddDays(-30);
            int recentCount = reflections.Count(r => r.UpdatedAt >= since)
                               + plts.Count(p => p.UpdatedAt >= since)
                               + learningNeeds.Count(l => l.UpdatedAt >= since)
                               + sles.Count(s => s.UpdatedAt >= since);

            // Build a concise summary
            sb.AppendLine($"Summary for {name} ({user?.Scheme?.Name ?? "No Scheme"}):");
            sb.AppendLine("- SLE completion:");
            foreach (var kvp in sleCounts)
            {
                var pct = kvp.Value.total == 0 ? 0 : (int)(100.0 * kvp.Value.completed / kvp.Value.total);
                sb.AppendLine($"  • {kvp.Key}: {kvp.Value.completed}/{kvp.Value.total} ({pct}%)");
            }
            sb.AppendLine($"- PLTs: {pltComplete}/{pltTotal}");
            sb.AppendLine($"- Reflections: {reflectionsComplete}/{reflectionsTotal}");
            sb.AppendLine($"- Learning Needs completed: {lnCompleted}/{lnTotal}");

            var recent = new[] { latestSLE, latestPLT, latestRefl, latestLN }.Max();
            if (recent != System.DateTime.MinValue)
            {
                sb.AppendLine($"- Last activity: {recent:dd MMM yyyy}");
            }
            sb.AppendLine($"- Activity in last 30 days: {recentCount}");
            if (topKeywords.Length > 0)
            {
                sb.AppendLine($"- Top themes: {string.Join(", ", topKeywords)}");
            }
            if (topEpas.Any())
            {
                sb.AppendLine($"- EPA coverage (top): {string.Join(", ", topEpas)}");
            }
            if (gapEpas.Any())
            {
                sb.AppendLine($"- EPA gaps: {string.Join(", ", gapEpas)}");
            }

            var highlights = new System.Collections.Generic.List<string>();
            // Simple highlight rules
            if (sleCounts.Any() && sleCounts.Average(k => k.Value.total == 0 ? 0 : (100.0 * k.Value.completed / k.Value.total)) >= 70)
                highlights.Add("Strong overall SLE completion");
            if (reflectionsTotal > 0 && reflectionsComplete == reflectionsTotal)
                highlights.Add("All reflections locked");
            if (lnTotal > 0 && lnCompleted == 0)
                highlights.Add("Learning needs identified but not completed");
            if (pltTotal == 0)
                highlights.Add("No PLT recorded");

            if (recentCount == 0)
                highlights.Add("No recent activity in the last 30 days");
            else if (recentCount >= 5)
                highlights.Add("Good recent activity (≥5 updates in 30 days)");

            if (topKeywords.Length > 0)
                highlights.Add($"Themes emerging: {string.Join(", ", topKeywords)}");

            if (gapEpas.Any())
                highlights.Add($"Consider adding evidence for EPA(s): {string.Join(", ", gapEpas)}");

            var result = new AIAnalysisResult
            {
                Title = $"Portfolio snapshot for {name}",
                Summary = sb.ToString().Trim(),
                Highlights = highlights.ToArray(),
                ESInsight = latestES != null ? BuildESInsight(latestES) : null
            };

            return result;
        }

        private static string[] ExtractTopKeywords(string text, int topN)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
            var separators = new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':', '!', '?', '"', '\'', '-', '(', ')', '[', ']', '{', '}', '/' };
            var tokens = text
                .ToLowerInvariant()
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Where(w => !StopWords.Contains(w))
                .GroupBy(w => w)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Word)
                .Take(topN)
                .Select(x => x.Word)
                .ToArray();
            return tokens;
        }

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Basic English stopwords plus common domain-neutral words
            "about","above","after","again","against","all","also","am","among","an","and","any","are","as","at",
            "be","because","been","before","being","below","between","both","but","by","can","cannot","could",
            "did","do","does","doing","down","during","each","few","for","from","further","had","has","have","having",
            "he","her","here","hers","herself","him","himself","his","how","however","i","if","in","into","is","it",
            "its","itself","just","least","less","like","made","make","many","may","might","more","most","mostly","much",
            "must","my","myself","no","nor","not","now","of","off","on","once","one","only","or","other","our","ours",
            "ourselves","out","over","own","same","she","should","since","so","some","such","than","that","the","their",
            "theirs","them","themselves","then","there","these","they","this","those","through","to","too","under","until",
            "up","very","was","we","were","what","when","where","which","while","who","whom","why","with","would","you",
            "your","yours","yourself","yourselves",
            // common portfolio terms to downweight
            "reflection","reflections","learning","needs","supervision","assessment","assessor","portfolio","event","case",
            "patient","clinic","clinical","skills","discussion","feedback","notes","report","time","teaching","teacher",
            "training","practice","work","team","teams","plan","plans","next","steps","objective","objectives","goal",
            "goals","area","areas","development","strengths","recommendations","comments","since","last","review"
        };

        private static string BuildESInsight(Models.AdHocESReport report)
        {
            var parts = new System.Collections.Generic.List<string>();
            void Add(string label, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    parts.Add($"{label}: {value}");
            }

            Add("Overall", report.ESOverallAssessment);
            Add("Strengths", report.ESStrengths);
            Add("Development", report.ESAreasForDevelopment);
            Add("Recommendations", report.ESRecommendations);
            Add("Additional", report.ESAdditionalComments);
            Add("Since last review", report.ESProgressSinceLastReview);
            Add("Clinical performance", report.ESClinicalPerformance);
            Add("Professional behavior", report.ESProfessionalBehavior);
            return string.Join(" \n", parts);
        }
    }
}
