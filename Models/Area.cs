namespace EYDGateway.Models
{
    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Scheme> Schemes { get; set; }
    }
}
