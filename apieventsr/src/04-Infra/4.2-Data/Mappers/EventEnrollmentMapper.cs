using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class EventEnrollmentMapper : IEntityTypeConfiguration<EventEnrollment>
    {
        public void Configure(EntityTypeBuilder<EventEnrollment> builder)
        {
            builder.ToTable("event_enrollments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ProjectName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ResponsibleName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ManagementRepresentative)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.AuthorId).IsRequired();
            builder.Property(e => e.SchoolId).IsRequired();

            // Regra: escola não pode ter dois projetos com mesmo segmento+categoria no mesmo evento
            // O filtro garante que projetos excluídos logicamente não entram na validação
            builder.HasIndex(e => new { e.SchoolId, e.EventId, e.SegmentId, e.CategoryId })
                .IsUnique()
                .HasFilter("\"DeleteDate\" IS NULL");

            builder.HasOne(e => e.Segment)
                .WithMany()
                .HasForeignKey(e => e.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Files)
                .WithOne(f => f.EventEnrollment)
                .HasForeignKey(f => f.EventEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
