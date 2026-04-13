using System.ComponentModel.DataAnnotations;

namespace apieventsr.Application.Dtos.Requests
{
    public class CreateEnrollmentRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid SegmentId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ResponsibleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ManagementRepresentative { get; set; } = string.Empty;
    }
}
