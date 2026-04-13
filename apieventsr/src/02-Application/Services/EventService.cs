using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using apieventsr.Domain.Enums;

namespace apieventsr.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<List<EventListItemResponse>> GetAllAsync(Guid? schoolId)
        {
            var events = await _eventRepository.GetAllAsync(schoolId);

            // Se tiver schoolId, buscamos quais eventos a escola já está inscrita
            var enrolledEventIds = new List<Guid>();
            if (schoolId.HasValue && events.Count > 0)
            {
                var eventIds = events.Select(e => e.Id).ToList();
                enrolledEventIds = await _eventRepository.GetEnrolledEventIdsBySchoolAsync(schoolId.Value, eventIds);
            }

            return events.Select(e => MapToListItem(e, enrolledEventIds)).ToList();
        }

        public async Task<EventDetailResponse> GetByIdAsync(Guid id, Guid? schoolId)
        {
            var evento = await _eventRepository.GetByIdAsync(id);

            if (evento is null)
                throw new KeyNotFoundException($"Evento {id} não encontrado.");

            var hasEnrollment = false;
            if (schoolId.HasValue)
            {
                var enrolledIds = await _eventRepository.GetEnrolledEventIdsBySchoolAsync(
                    schoolId.Value, new List<Guid> { id });
                hasEnrollment = enrolledIds.Contains(id);
            }

            var status = CalculateStatus(evento);

            return new EventDetailResponse
            {
                Id = evento.Id,
                Title = evento.Title,
                Description = evento.Description,
                BannerUrl = evento.BannerUrl,
                AwardDetails = evento.AwardDetails,
                Status = status,
                HasEnrollment = hasEnrollment,
                CanEnroll = status == EventStatus.EnrollmentOpen,
                CanViewProjects = status != EventStatus.ComingSoon,
                Documents = evento.Documents
                    .OrderBy(d => d.Name)
                    .Select(d => new EventDocumentResponse
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Url = d.Url
                    }).ToList(),
                Schedule = BuildSchedule(evento)
            };
        }

        // Monta o cronograma a partir das datas do evento
        private static List<ScheduleItemResponse> BuildSchedule(Event e)
        {
            return new List<ScheduleItemResponse>
            {
                new() { Label = "Início das inscrições", Date = e.EnrollmentStartDate },
                new() { Label = "Fim das inscrições",   Date = e.EnrollmentEndDate },
                new() { Label = "Divulgação",            Date = e.ResultDate }
            };
        }

        // Monta o DTO de cada evento com status e flags calculados
        private static EventListItemResponse MapToListItem(Event e, List<Guid> enrolledEventIds)
        {
            var status = CalculateStatus(e);

            return new EventListItemResponse
            {
                Id = e.Id,
                Title = e.Title,
                BannerUrl = e.BannerUrl,
                EnrollmentStartDate = e.EnrollmentStartDate,
                EnrollmentEndDate = e.EnrollmentEndDate,
                Status = status,
                HasEnrollment = enrolledEventIds.Contains(e.Id),
                IsCurrent = status != EventStatus.Closed
            };
        }

        // Motor de status: calculado a partir das datas do evento
        // Usa <= e >= (inclusive): quando a data chega, o status já virou
        private static EventStatus CalculateStatus(Event e)
        {
            var now = DateTime.UtcNow;

            if (now < e.EnrollmentStartDate)
                return EventStatus.ComingSoon;     // Em breve

            if (now <= e.EnrollmentEndDate)
                return EventStatus.EnrollmentOpen; // Inscrição aberta

            if (now < e.ResultDate)
                return EventStatus.InProgress;     // Em andamento

            return EventStatus.Closed;             // Encerrado
        }
    }
}
