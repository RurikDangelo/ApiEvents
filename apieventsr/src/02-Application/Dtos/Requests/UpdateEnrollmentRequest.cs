using System.ComponentModel.DataAnnotations;

namespace apieventsr.Application.Dtos.Requests
{
    public class UpdateEnrollmentRequest
    {
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
