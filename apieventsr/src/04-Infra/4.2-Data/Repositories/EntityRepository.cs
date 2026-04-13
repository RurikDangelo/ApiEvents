using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;

namespace apieventsr.Data.Repositories
{
    public class EntityRepository : IEntityRepository
    {
        private readonly ProjectContext _dbContext;

        public EntityRepository(ProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DomainEntity> GetEntity(int parameter)
        {
            return new DomainEntity(parameter);
        }
    }
}
