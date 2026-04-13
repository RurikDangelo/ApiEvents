using apieventsr.Application.Dtos.Responses;

namespace apieventsr.Application.Interfaces
{
    public interface IEventService
    {
        Task<List<EventListItemResponse>> GetAllAsync(Guid? schoolId);
        Task<EventDetailResponse> GetByIdAsync(Guid id, Guid? schoolId);
    }
}
