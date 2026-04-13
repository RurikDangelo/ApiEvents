using apieventsr.Domain.Entities;

namespace apieventsr.Data.Interfaces
{
    public interface IEntityRepository
    {
        public Task<DomainEntity> GetEntity(int parameter);
    }
}
