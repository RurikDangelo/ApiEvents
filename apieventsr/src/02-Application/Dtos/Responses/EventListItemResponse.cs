using apieventsr.Domain.Enums;

namespace apieventsr.Application.Dtos.Responses
{
    public class EventListItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public DateTime EnrollmentStartDate { get; set; }
        public DateTime EnrollmentEndDate { get; set; }

        // Status calculado com base nas datas (não vem do banco)
        public EventStatus Status { get; set; }

        // Etiqueta extra: escola já tem projeto inscrito neste evento
        public bool HasEnrollment { get; set; }

        // Flag usada pelo frontend para separar as seções da tela:
        // true  → seção "Eventos em andamento"
        // false → seção "Eventos anteriores"
        public bool IsCurrent { get; set; }
    }
}
