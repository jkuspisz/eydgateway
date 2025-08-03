namespace EYDGateway.Models
{
    public interface IEPAMappable
    {
        int Id { get; }
        string UserId { get; }
        ICollection<EPAMapping> EPAMappings { get; set; }
    }
}
