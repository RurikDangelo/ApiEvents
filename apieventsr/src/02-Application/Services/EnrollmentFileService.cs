using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using apieventsr.Domain.Enums;

namespace apieventsr.Application.Services
{
    public class EnrollmentFileService : IEnrollmentFileService
    {
        private const int MaxTotalFiles  = 6;
        private const int MaxImageFiles  = 4;
        private const int MaxPdfFiles    = 1;
        private const int MaxVideoFiles  = 1;

        private static readonly Dictionary<string, FileType> AllowedContentTypes = new()
        {
            { "image/jpeg",       FileType.Image },
            { "image/png",        FileType.Image },
            { "image/gif",        FileType.Image },
            { "image/webp",       FileType.Image },
            { "application/pdf",  FileType.Pdf   },
            { "video/mp4",        FileType.Video },
            { "video/webm",       FileType.Video },
            { "video/quicktime",  FileType.Video },
        };

        private readonly IEnrollmentFileRepository _fileRepo;
        private readonly IEventEnrollmentRepository _enrollmentRepo;
        private readonly IFileStorageService _storage;

        public EnrollmentFileService(
            IEnrollmentFileRepository fileRepo,
            IEventEnrollmentRepository enrollmentRepo,
            IFileStorageService storage)
        {
            _fileRepo       = fileRepo;
            _enrollmentRepo = enrollmentRepo;
            _storage        = storage;
        }

        public async Task<UploadFileResponse> UploadAsync(
            Guid enrollmentId,
            Stream fileStream,
            string fileName,
            string contentType,
            long sizeInBytes,
            Guid schoolId)
        {
            // 1. Inscrição existe e pertence à escola?
            var enrollment = await _enrollmentRepo.GetByIdAsync(enrollmentId);
            if (enrollment is null)
                throw new KeyNotFoundException($"Inscrição {enrollmentId} não encontrada.");

            if (enrollment.SchoolId != schoolId)
                throw new InvalidOperationException("Você não tem permissão para enviar arquivos nesta inscrição.");

            // 2. Evento ainda permite upload?
            var now = DateTime.UtcNow;
            var ev = enrollment.Event;
            var isClosed = now >= ev.ResultDate;
            if (isClosed)
                throw new InvalidOperationException("Não é possível enviar arquivos para inscrições de eventos encerrados.");

            // 3. Tipo de arquivo permitido?
            var normalizedType = contentType.ToLowerInvariant();
            if (!AllowedContentTypes.TryGetValue(normalizedType, out var fileType))
                throw new InvalidOperationException(
                    $"Tipo de arquivo não permitido: {normalizedType}. " +
                    "Aceitos: JPEG, PNG, GIF, WEBP, PDF, MP4, WEBM, MOV.");

            // 4. Valida limites por tipo
            var existing = await _fileRepo.GetByEnrollmentIdAsync(enrollmentId);

            if (existing.Count >= MaxTotalFiles)
                throw new InvalidOperationException($"Limite de {MaxTotalFiles} arquivos por inscrição atingido.");

            var imageCount = existing.Count(f => f.FileType == FileType.Image);
            var pdfCount   = existing.Count(f => f.FileType == FileType.Pdf);
            var videoCount = existing.Count(f => f.FileType == FileType.Video);

            if (fileType == FileType.Image && imageCount >= MaxImageFiles)
                throw new InvalidOperationException($"Limite de {MaxImageFiles} imagens por inscrição atingido.");

            if (fileType == FileType.Pdf && pdfCount >= MaxPdfFiles)
                throw new InvalidOperationException("Já existe um PDF nesta inscrição. Remova-o antes de enviar outro.");

            if (fileType == FileType.Video && videoCount >= MaxVideoFiles)
                throw new InvalidOperationException("Já existe um vídeo nesta inscrição. Remova-o antes de enviar outro.");

            // 5. Nome único por projeto (o índice do banco vai barrar também, mas respondemos de forma amigável)
            var nameExists = existing.Any(f => f.OriginalName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (nameExists)
                throw new InvalidOperationException($"Já existe um arquivo com o nome '{fileName}' nesta inscrição.");

            // 6. Salva no disco
            var timestamp   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var storageName = $"{timestamp}_{SanitizeFileName(fileName)}";
            var blobName    = await _storage.SaveAsync(fileStream, storageName, enrollmentId);

            // 7. Persiste no banco
            var enrollmentFile = new EnrollmentFile
            {
                OriginalName        = fileName,
                StorageName         = storageName,
                BlobName            = blobName,
                ContentType         = normalizedType,
                FileType            = fileType,
                SizeInBytes         = sizeInBytes,
                EventEnrollmentId   = enrollmentId
            };

            var created = await _fileRepo.CreateAsync(enrollmentFile);

            return new UploadFileResponse
            {
                Id           = created.Id,
                OriginalName = created.OriginalName,
                BlobName     = created.BlobName,
                ContentType  = created.ContentType,
                FileType     = created.FileType,
                SizeInBytes  = created.SizeInBytes,
                UploadedAt   = created.CreateDate
            };
        }

        public async Task DeleteAsync(Guid enrollmentId, Guid fileId, Guid schoolId)
        {
            var file = await _fileRepo.GetByIdAsync(fileId);
            if (file is null)
                throw new KeyNotFoundException($"Arquivo {fileId} não encontrado.");

            if (file.EventEnrollmentId != enrollmentId)
                throw new InvalidOperationException("O arquivo não pertence à inscrição informada.");

            if (file.EventEnrollment.SchoolId != schoolId)
                throw new InvalidOperationException("Você não tem permissão para remover este arquivo.");

            // Remove físico primeiro; se falhar, não registra a exclusão lógica
            await _storage.DeleteAsync(file.BlobName);
            await _fileRepo.DeleteAsync(file);
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        }
    }
}
