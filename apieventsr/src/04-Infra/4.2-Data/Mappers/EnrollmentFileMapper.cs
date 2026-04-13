using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class EnrollmentFileMapper : IEntityTypeConfiguration<EnrollmentFile>
    {
        public void Configure(EntityTypeBuilder<EnrollmentFile> builder)
        {
            builder.ToTable("enrollment_files");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.OriginalName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.StorageName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.BlobName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.FileType).IsRequired();
            builder.Property(f => f.SizeInBytes).IsRequired();

            // Dois arquivos com o mesmo nome não podem existir no mesmo projeto
            builder.HasIndex(f => new { f.EventEnrollmentId, f.OriginalName })
                .IsUnique()
                .HasFilter("\"DeleteDate\" IS NULL");
        }
    }
}
