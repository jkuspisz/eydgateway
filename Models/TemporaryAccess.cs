namespace EYDGateway.Models
{
    public class TemporaryAccess
    {
        public int Id { get; set; }
        public string RequestingUserId { get; set; }  // TPD/Dean requesting access
        public string TargetEYDUserId { get; set; }   // EYD they want to access
        public string Reason { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public string? ApprovedByUserId { get; set; }
        
        // Navigation properties
        public ApplicationUser RequestingUser { get; set; }
        public ApplicationUser TargetEYDUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
