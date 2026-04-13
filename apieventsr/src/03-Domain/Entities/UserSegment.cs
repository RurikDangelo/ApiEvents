namespace apieventsr.Domain.Entities
{
    // Tabela de vínculo entre usuário (JWT) e segmento
    // Usada para validar permissões de Coordenador e Professor
    public class UserSegment : BaseEntity
    {
        public Guid UserId { get; set; }    // Id do usuário vindo do JWT
        public Guid SegmentId { get; set; }

        // Navegação
        public Segment Segment { get; set; } = null!;
    }
}
