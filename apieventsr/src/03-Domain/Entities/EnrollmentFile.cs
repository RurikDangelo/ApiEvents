using apieventsr.Domain.Enums;

namespace apieventsr.Domain.Entities
{
    // Arquivo associado a uma inscrição
    // Nome salvo no storage como: {schoolId}_{enrollmentId}_{originalName}
    public class EnrollmentFile : BaseEntity
    {
        public string OriginalName { get; set; } = string.Empty; // nome original do arquivo
        public string StorageName { get; set; } = string.Empty;  // nome formatado no storage
        public string BlobName { get; set; } = string.Empty;     // chave no storage
        public string ContentType { get; set; } = string.Empty;  // ex: image/jpeg, application/pdf
        public long SizeInBytes { get; set; }
        public FileType FileType { get; set; }

        // FK para EventEnrollment
        public Guid EventEnrollmentId { get; set; }

        // Navegação
        public EventEnrollment EventEnrollment { get; set; } = null!;
    }
}
