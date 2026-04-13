using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace apieventsr.Data.Mappers
{
    public class EventDocumentMapper : IEntityTypeConfiguration<EventDocument>
    {
        public void Configure(EntityTypeBuilder<EventDocument> builder)
        {
            builder.ToTable("event_documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.BlobName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.Url)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}
