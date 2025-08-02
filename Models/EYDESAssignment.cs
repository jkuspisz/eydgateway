namespace EYDGateway.Models
{
    public class EYDESAssignment
    {
        public int Id { get; set; }
        public string EYDUserId { get; set; }
        public string ESUserId { get; set; }
        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public ApplicationUser EYDUser { get; set; }
        public ApplicationUser ESUser { get; set; }
    }
}
