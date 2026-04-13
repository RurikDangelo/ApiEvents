using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class EventMapper : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired();

            builder.Property(e => e.BannerUrl)
                .HasMaxLength(500);

            builder.Property(e => e.EnrollmentStartDate).IsRequired();
            builder.Property(e => e.EnrollmentEndDate).IsRequired();
            builder.Property(e => e.ResultDate).IsRequired();

            builder.Property(e => e.AwardDetails);  // nullable, sem tamanho fixo

            builder.HasMany(e => e.Documents)
                .WithOne(d => d.Event)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Enrollments)
                .WithOne(en => en.Event)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
