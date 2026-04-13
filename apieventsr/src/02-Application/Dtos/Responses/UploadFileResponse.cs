using apieventsr.Domain.Enums;

namespace apieventsr.Application.Dtos.Responses
{
    public class UploadFileResponse
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
