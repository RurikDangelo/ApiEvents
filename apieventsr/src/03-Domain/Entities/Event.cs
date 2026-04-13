namespace apieventsr.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }

        // Datas que controlam o status calculado
        public DateTime EnrollmentStartDate { get; set; } // Início das inscrições
        public DateTime EnrollmentEndDate { get; set; }   // Fim das inscrições
        public DateTime ResultDate { get; set; }          // Divulgação / Encerramento

        // Texto descritivo da premiação (pode ser null se o evento não tiver prêmios)
        public string? AwardDetails { get; set; }

        // Navegação
        public ICollection<EventDocument> Documents { get; set; } = new List<EventDocument>();
        public ICollection<EventEnrollment> Enrollments { get; set; } = new List<EventEnrollment>();
    }
}
