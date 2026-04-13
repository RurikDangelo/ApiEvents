namespace apieventsr.Application.Dtos.Responses
{
    public class EnrollmentCreatedResponse
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string SegmentName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
