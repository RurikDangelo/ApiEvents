using apieventsr.Application.Dtos.Responses;

namespace apieventsr.Application.Interfaces
{
    public interface IService
    {
        public Task<EntityResponse> GetEntity(int application_id);
    }
}
