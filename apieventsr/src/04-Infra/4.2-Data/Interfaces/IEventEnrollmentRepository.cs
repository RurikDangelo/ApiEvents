using apieventsr.Domain.Entities;

namespace apieventsr.Data.Interfaces
{
    public interface IEventEnrollmentRepository
    {
        // Retorna segmentos ativos com suas categorias ativas.
        // Se filterByUser=true, limita aos segmentos vinculados ao userId (Coordenador/Professor)
        Task<List<Segment>> GetSegmentsWithCategoriesAsync(Guid? userId, bool filterByUser);

        // Retorna pares (SegmentId, CategoryId) que já estão inscritos pela escola neste evento
        Task<HashSet<(Guid SegmentId, Guid CategoryId)>> GetEnrolledPairsAsync(Guid eventId, Guid schoolId);

        // Retorna o evento pelo ID (para validar status antes de inscrever)
        Task<Event?> GetEventByIdAsync(Guid eventId);

        // Verifica se o segmento e a categoria existem, estão ativos e pertencem um ao outro
        Task<(Segment? Segment, Category? Category)> GetActiveSegmentCategoryAsync(Guid segmentId, Guid categoryId);

        // Verifica se já existe inscrição ativa da escola neste evento+segmento+categoria
        Task<bool> EnrollmentExistsAsync(Guid eventId, Guid schoolId, Guid segmentId, Guid categoryId);

        // Persiste a nova inscrição
        Task<EventEnrollment> CreateAsync(EventEnrollment enrollment);

        // Retorna todas as inscrições ativas da escola com Event, Segment e Category carregados
        Task<List<EventEnrollment>> GetAllBySchoolAsync(Guid schoolId);

        // Retorna os IDs de segmentos que um usuário gerencia (para filtro de Coordenador)
        Task<List<Guid>> GetUserSegmentIdsAsync(Guid userId);

        // Retorna uma inscrição pelo ID com Event, Segment e Category carregados
        Task<EventEnrollment?> GetByIdAsync(Guid id);

        // Persiste alterações em uma inscrição existente
        Task<EventEnrollment> UpdateAsync(EventEnrollment enrollment);

        // Exclusão lógica: seta DeleteDate via BaseEntity.Delete()
        Task DeleteAsync(EventEnrollment enrollment);
    }
}
