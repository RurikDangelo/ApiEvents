using apieventsr.Domain.Enums;

namespace apieventsr.Application.Dtos.Responses
{
    public class EventDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }

        // Status calculado a partir das datas
        public EventStatus Status { get; set; }

        // Etiqueta extra: escola já tem pelo menos uma inscrição neste evento
        public bool HasEnrollment { get; set; }

        // Documentos do evento (regulamento, edital etc.) ordenados por nome
        public List<EventDocumentResponse> Documents { get; set; } = new();

        // Cronograma derivado das datas do evento
        public List<ScheduleItemResponse> Schedule { get; set; } = new();

        // Detalhes da premiação (null = evento sem premiação)
        public string? AwardDetails { get; set; }

        // Flags de navegação para o frontend
        // true quando status == EnrollmentOpen
        public bool CanEnroll { get; set; }

        // true quando o evento já teve inscrições abertas (qualquer status exceto ComingSoon)
        public bool CanViewProjects { get; set; }
    }
}
