namespace apieventsr.Domain.Entities
{
    public class Segment : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Navegação
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<UserSegment> UserSegments { get; set; } = new List<UserSegment>();
    }
}
