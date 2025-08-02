namespace EYDGateway.Models
{
    public class Scheme
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int AreaId { get; set; }
        public Area Area { get; set; }
    }
}
