using apieventsr.Domain.Entities;

namespace apieventsr.Data.Interfaces
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllAsync(Guid? schoolId);
        Task<List<Guid>> GetEnrolledEventIdsBySchoolAsync(Guid schoolId, List<Guid> eventIds);
        Task<Event?> GetByIdAsync(Guid id);
    }
}
