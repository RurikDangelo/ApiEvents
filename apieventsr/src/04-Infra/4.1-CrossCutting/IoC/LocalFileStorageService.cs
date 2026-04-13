using apieventsr.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace apieventsr.IoC
{
    // Implementação local: salva arquivos no disco em {FILE_STORAGE_PATH}/{enrollmentId}/{storageName}
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            var configured = configuration["FILE_STORAGE_PATH"];
            _basePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
                : configured;
        }

        public async Task<string> SaveAsync(Stream fileStream, string storageName, Guid enrollmentId)
        {
            var dir = Path.Combine(_basePath, enrollmentId.ToString());
            Directory.CreateDirectory(dir);

            var physicalPath = Path.Combine(dir, storageName);

            using var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs);

            return Path.Combine(enrollmentId.ToString(), storageName).Replace("\\", "/");
        }

        public Task DeleteAsync(string blobName)
        {
            var physicalPath = Path.Combine(_basePath, blobName.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }
    }
}
