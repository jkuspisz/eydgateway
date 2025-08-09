using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class MSFResultsDto
    {
        public MSFQuestionnaire Questionnaire { get; set; } = new();
        public int TotalResponses { get; set; }
        public string PerformerName { get; set; } = "";
        public string FeedbackUrl { get; set; } = "";
        public string QRCodeDataUrl { get; set; } = "";

        // Statistical Analysis
        public Dictionary<string, double> QuestionAverages { get; set; } = new();
        public double OverallAverage { get; set; }
        public Dictionary<string, Dictionary<int, int>> ScoreDistributions { get; set; } = new();

        // Topic-based averages
        public double CommunicationAverage { get; set; }
        public double ProfessionalismAverage { get; set; }
        public double ManagementLeadershipAverage { get; set; }

        // Text Feedback
        public List<string> PositiveComments { get; set; } = new();
        public List<string> ImprovementComments { get; set; } = new();

        // Recent Activity
        public List<MSFResponse> RecentResponses { get; set; } = new();
    }
}
