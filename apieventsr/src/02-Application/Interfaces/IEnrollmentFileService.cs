using apieventsr.Application.Dtos.Responses;

namespace apieventsr.Application.Interfaces
{
    public interface IEnrollmentFileService
    {
        // Faz upload de um arquivo para a inscrição.
        // Valida limites (6 total, 4 imagens, 1 PDF, 1 vídeo), tipo e unicidade do nome.
        Task<UploadFileResponse> UploadAsync(
            Guid enrollmentId,
            Stream fileStream,
            string fileName,
            string contentType,
            long sizeInBytes,
            Guid schoolId);

        // Exclusão lógica do registro + remoção do arquivo físico.
        Task DeleteAsync(Guid enrollmentId, Guid fileId, Guid schoolId);
    }
}
