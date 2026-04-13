namespace apieventsr.Application.Dtos.Responses
{
    // Representa uma entrada do cronograma do evento (label + data)
    public class ScheduleItemResponse
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
