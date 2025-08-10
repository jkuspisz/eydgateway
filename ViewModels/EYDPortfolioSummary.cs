using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class EYDPortfolioSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string SchemeName { get; set; } = string.Empty;
        
        // Individual SLE Type Summaries (like in EYD Portfolio)
        public int CBDCompleted { get; set; }
        public int CBDTotal { get; set; }
        public string CBDStatus { get; set; } = "not-started";
        
        public int DOPSCompleted { get; set; }
        public int DOPSTotal { get; set; }
        public string DOPSStatus { get; set; } = "not-started";
        
        public int MiniCEXCompleted { get; set; }
        public int MiniCEXTotal { get; set; }
        public string MiniCEXStatus { get; set; } = "not-started";
        
        public int DOPSSimCompleted { get; set; }
        public int DOPSSimTotal { get; set; }
        public string DOPSSimStatus { get; set; } = "not-started";
        
        public int DtCTCompleted { get; set; }
        public int DtCTTotal { get; set; }
        public string DtCTStatus { get; set; } = "not-started";
        
        public int DENTLCompleted { get; set; }
        public int DENTLTotal { get; set; }
        public string DENTLStatus { get; set; } = "not-started";
        
        // PLT Summary
        public int PLTCompleted { get; set; }
        public int PLTTotal { get; set; }
        public string PLTStatus { get; set; } = "not-started";
        
        // Reflection Summary
        public int ReflectionCompleted { get; set; }
        public int ReflectionTotal { get; set; }
        public string ReflectionStatus { get; set; } = "not-started";
        
        // Learning Need Summary
        public int LearningNeedCompleted { get; set; }
        public int LearningNeedTotal { get; set; }
        public string LearningNeedStatus { get; set; } = "not-started";
        
        // IRCP/FRCP Status (matching EYD Portfolio format)
        public string IRCPESStatus { get; set; } = "NotStarted";
        public string IRCPEYDStatus { get; set; } = "NotStarted";
        public string IRCPPanelStatus { get; set; } = "NotStarted";
        
        public string FRCPESStatus { get; set; } = "NotStarted";
        public string FRCPEYDStatus { get; set; } = "NotStarted";
        public string FRCPPanelStatus { get; set; } = "NotStarted";
        
        // Helper method to calculate status based on completion
        private string CalculateStatus(int completed, int total)
        {
            if (total == 0) return "not-started";
            if (completed >= total) return "complete";
            if (completed > 0) return "in-progress";
            return "not-started";
        }
        
        // Method to update all statuses
        public void UpdateStatuses()
        {
            CBDStatus = CalculateStatus(CBDCompleted, CBDTotal);
            DOPSStatus = CalculateStatus(DOPSCompleted, DOPSTotal);
            MiniCEXStatus = CalculateStatus(MiniCEXCompleted, MiniCEXTotal);
            DOPSSimStatus = CalculateStatus(DOPSSimCompleted, DOPSSimTotal);
            DtCTStatus = CalculateStatus(DtCTCompleted, DtCTTotal);
            DENTLStatus = CalculateStatus(DENTLCompleted, DENTLTotal);
            PLTStatus = CalculateStatus(PLTCompleted, PLTTotal);
            ReflectionStatus = CalculateStatus(ReflectionCompleted, ReflectionTotal);
            LearningNeedStatus = CalculateStatus(LearningNeedCompleted, LearningNeedTotal);
        }
    }
}
