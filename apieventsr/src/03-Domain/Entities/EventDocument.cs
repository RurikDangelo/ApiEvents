namespace apieventsr.Domain.Entities
{
    // Documentos anexados ao evento (ex: regulamento, edital)
    public class EventDocument : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty; // chave no storage
        public string Url { get; set; } = string.Empty;      // URL pública de acesso

        // FK para Event
        public Guid EventId { get; set; }

        // Navegação
        public Event Event { get; set; } = null!;
    }
}
