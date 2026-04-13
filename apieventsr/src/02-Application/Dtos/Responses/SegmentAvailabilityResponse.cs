namespace apieventsr.Application.Dtos.Responses
{
    public class SegmentAvailabilityResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<CategoryAvailabilityItem> Categories { get; set; } = new();
    }
}
