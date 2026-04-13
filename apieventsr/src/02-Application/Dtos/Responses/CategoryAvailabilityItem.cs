namespace apieventsr.Application.Dtos.Responses
{
    public class CategoryAvailabilityItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // false quando a escola já tem uma inscrição ativa neste segmento+categoria
        public bool IsAvailable { get; set; }
    }
}
