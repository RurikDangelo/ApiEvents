using apieventsr.Application.Dtos.Requests;
using apieventsr.Application.Dtos.Responses;

namespace apieventsr.Application.Interfaces
{
    public interface IEventEnrollmentService
    {
        // eventId   → evento para o qual queremos as opções de inscrição
        // schoolId  → escola do usuário logado (para marcar o que já está inscrito)
        // userId    → usuário logado (para filtrar por segmento, se Coordenador/Professor)
        // filterByUser → true se o perfil do usuário exige filtro por segmento vinculado
        Task<List<SegmentAvailabilityResponse>> GetAvailableSegmentsAsync(
            Guid eventId, Guid? schoolId, Guid? userId, bool filterByUser);

        // Cria uma nova inscrição. schoolId e authorId vêm do JWT (nunca do body).
        Task<EnrollmentCreatedResponse> CreateAsync(
            CreateEnrollmentRequest request, Guid schoolId, Guid authorId);

        // Lista as inscrições da escola com flags de permissão baseadas no perfil do usuário.
        // role: "school" | "coordinator" | "professor"
        Task<List<EnrollmentListItemResponse>> GetAllAsync(
            Guid schoolId, Guid? userId, string role);

        // Edita os campos de uma inscrição existente.
        // schoolId, userId e role são do JWT — usados para validar permissão.
        Task<EnrollmentCreatedResponse> UpdateAsync(
            Guid id, UpdateEnrollmentRequest request, Guid schoolId, Guid? userId, string role);

        // Exclusão lógica. Mesmas regras de permissão do UpdateAsync.
        Task DeleteAsync(Guid id, Guid schoolId, Guid? userId, string role);
    }
}
