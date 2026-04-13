using apieventsr.Domain.Enums;

namespace apieventsr.Application.Dtos.Responses
{
    public class EnrollmentListItemResponse
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string SegmentName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public EventStatus EventStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // Flags de permissão calculadas no servidor (baseadas no status do evento e perfil do usuário)
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUpload { get; set; }
    }
}
