using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace apieventsr.Data.Mappers
{
    public class EntityMapper : IEntityTypeConfiguration<DomainEntity>
    {
        public void Configure(EntityTypeBuilder<DomainEntity> modelBuilder)
        {

        }
    }
}
