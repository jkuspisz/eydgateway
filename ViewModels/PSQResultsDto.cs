using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class PSQResultsDto
    {
        public PSQQuestionnaire Questionnaire { get; set; } = new();
        public int TotalResponses { get; set; }
        public string PerformerName { get; set; } = "";
        public string FeedbackUrl { get; set; } = "";
        public string QRCodeDataUrl { get; set; } = "";

        // Question averages (excluding "Not observed" = 999)
        public Dictionary<string, double> QuestionAverages { get; set; } = new();
        public double OverallAverage { get; set; }

        // Response counts for each question
        public Dictionary<string, Dictionary<int, int>> ScoreDistributions { get; set; } = new();

        // Text feedback
        public List<string> PositiveComments { get; set; } = new();
        public List<string> ImprovementComments { get; set; } = new();

        // Recent responses
        public List<PSQResponse> RecentResponses { get; set; } = new();
    }
}
