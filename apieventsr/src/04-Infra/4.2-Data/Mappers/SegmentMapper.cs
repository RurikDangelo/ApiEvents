using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class SegmentMapper : IEntityTypeConfiguration<Segment>
    {
        public void Configure(EntityTypeBuilder<Segment> builder)
        {
            builder.ToTable("segments");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.IsActive).IsRequired();

            builder.HasMany(s => s.Categories)
                .WithOne(c => c.Segment)
                .HasForeignKey(c => c.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.UserSegments)
                .WithOne(us => us.Segment)
                .HasForeignKey(us => us.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
