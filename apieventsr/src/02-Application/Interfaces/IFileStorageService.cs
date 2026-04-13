namespace apieventsr.Application.Interfaces
{
    public interface IFileStorageService
    {
        // Salva o arquivo e retorna o BlobName (caminho relativo dentro do storage)
        Task<string> SaveAsync(Stream fileStream, string storageName, Guid enrollmentId);

        // Remove o arquivo físico pelo BlobName
        Task DeleteAsync(string blobName);
    }
}
