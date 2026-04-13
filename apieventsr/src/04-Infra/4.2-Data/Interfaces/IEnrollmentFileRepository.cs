using apieventsr.Domain.Entities;

namespace apieventsr.Data.Interfaces
{
    public interface IEnrollmentFileRepository
    {
        // Retorna todos os arquivos ativos de uma inscrição
        Task<List<EnrollmentFile>> GetByEnrollmentIdAsync(Guid enrollmentId);

        // Persiste o novo arquivo
        Task<EnrollmentFile> CreateAsync(EnrollmentFile file);

        // Retorna um arquivo pelo ID (para validação antes de deletar)
        Task<EnrollmentFile?> GetByIdAsync(Guid id);

        // Exclusão lógica
        Task DeleteAsync(EnrollmentFile file);
    }
}
