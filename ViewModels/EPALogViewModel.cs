using EYDGateway.Models;

namespace EYDGateway.ViewModels
{
    public class EPALogViewModel
    {
        public List<EPA> EPAs { get; set; } = new List<EPA>();
        public List<ActivityColumn> ActivityColumns { get; set; } = new List<ActivityColumn>();
        public EPAActivityMatrix Matrix { get; set; } = new EPAActivityMatrix();
        public string UserName { get; set; } = string.Empty;
        public DateTime? LastActivity { get; set; }
        public EPAProgressSummary Summary { get; set; } = new EPAProgressSummary();
    }

    public class ActivityColumn
    {
        public string EntityType { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }

    public class EPAActivityMatrix
    {
        // Dictionary: EPA.Id -> EntityType -> Count
        public Dictionary<int, Dictionary<string, EPAActivityCell>> Data { get; set; } = new Dictionary<int, Dictionary<string, EPAActivityCell>>();
        
        public EPAActivityCell GetCell(int epaId, string entityType)
        {
            if (Data.ContainsKey(epaId) && Data[epaId].ContainsKey(entityType))
                return Data[epaId][entityType];
            
            return new EPAActivityCell { Count = 0, Activities = new List<ActivitySummary>() };
        }
        
        public void SetCell(int epaId, string entityType, EPAActivityCell cell)
        {
            if (!Data.ContainsKey(epaId))
                Data[epaId] = new Dictionary<string, EPAActivityCell>();
            
            Data[epaId][entityType] = cell;
        }
    }

    public class EPAActivityCell
    {
        public int Count { get; set; }
        public List<ActivitySummary> Activities { get; set; } = new List<ActivitySummary>();
        public DateTime? LatestDate { get; set; }
        public string IntensityClass { get; set; } = string.Empty; // For color coding based on count
    }

    public class ActivitySummary
    {
        public int EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
    }

    public class EPAProgressSummary
    {
        public int TotalActivities { get; set; }
        public int TotalEPAMappings { get; set; }
        public int EPAsWithActivity { get; set; } // EPAs that have at least 1 activity
        public int EPAsNotStarted { get; set; } // EPAs with 0 activities
        public string MostActiveEPA { get; set; } = string.Empty;
        public string LeastActiveEPA { get; set; } = string.Empty;
        public Dictionary<string, int> ActivityTypeTotals { get; set; } = new Dictionary<string, int>();
    }

    // Static helper for activity type configuration
    public static class ActivityTypes
    {
        public static List<ActivityColumn> GetStandardColumns()
        {
            return new List<ActivityColumn>
            {
                new ActivityColumn { EntityType = "Reflection", DisplayName = "Reflections", ShortName = "Refl", CssClass = "bg-primary", TotalCount = 0 },
                new ActivityColumn { EntityType = "SLE", DisplayName = "Supervised Learning Events", ShortName = "SLE", CssClass = "bg-success", TotalCount = 0 },
                new ActivityColumn { EntityType = "ProtectedLearningTime", DisplayName = "Protected Learning Time", ShortName = "PLT", CssClass = "bg-info", TotalCount = 0 },
                new ActivityColumn { EntityType = "SignificantEvent", DisplayName = "Significant Events", ShortName = "SE", CssClass = "bg-warning", TotalCount = 0 },
                new ActivityColumn { EntityType = "QIUpload", DisplayName = "Quality Improvement", ShortName = "QI", CssClass = "bg-secondary", TotalCount = 0 }
            };
        }
    }
}
