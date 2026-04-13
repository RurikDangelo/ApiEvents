using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class UserSegmentMapper : IEntityTypeConfiguration<UserSegment>
    {
        public void Configure(EntityTypeBuilder<UserSegment> builder)
        {
            builder.ToTable("user_segments");

            builder.HasKey(us => us.Id);

            builder.Property(us => us.UserId).IsRequired();
            builder.Property(us => us.SegmentId).IsRequired();

            // Um usuário não pode ter o mesmo segmento vinculado duas vezes
            builder.HasIndex(us => new { us.UserId, us.SegmentId }).IsUnique();
        }
    }
}
