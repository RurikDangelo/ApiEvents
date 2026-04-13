namespace apieventsr.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // FK para Segment
        public Guid SegmentId { get; set; }

        // Navegação
        public Segment Segment { get; set; } = null!;
    }
}
