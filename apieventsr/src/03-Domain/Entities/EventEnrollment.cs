namespace apieventsr.Domain.Entities
{
    // Inscrição de um projeto no evento (1 escola pode ter N inscrições por evento,
    // mas nunca duas com o mesmo segmento + categoria)
    public class EventEnrollment : BaseEntity
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ResponsibleName { get; set; } = string.Empty;
        public string ManagementRepresentative { get; set; } = string.Empty;

        // Quem criou (userId vindo do JWT)
        public Guid AuthorId { get; set; }

        // Escola do usuário logado (schoolId vindo do JWT)
        public Guid SchoolId { get; set; }

        // FK para Event
        public Guid EventId { get; set; }

        // FK para Segment e Category
        public Guid SegmentId { get; set; }
        public Guid CategoryId { get; set; }

        // Navegação
        public Event Event { get; set; } = null!;
        public Segment Segment { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<EnrollmentFile> Files { get; set; } = new List<EnrollmentFile>();
    }
}
