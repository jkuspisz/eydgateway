using System.Threading.Tasks;

namespace EYDGateway.Services
{
    public interface IAIAnalysisService
    {
        Task<AIAnalysisResult> AnalyzePortfolioAsync(string eydUserId, string mode = "overall");
    }

    public class AIAnalysisResult
    {
        public string Title { get; set; } = "Portfolio Analysis";
        public string Summary { get; set; } = string.Empty;
        public string? ESInsight { get; set; }
        public string[] Highlights { get; set; } = System.Array.Empty<string>();
    }
}
