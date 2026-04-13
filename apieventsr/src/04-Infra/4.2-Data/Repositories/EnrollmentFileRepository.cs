using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace apieventsr.Data.Repositories
{
    public class EnrollmentFileRepository : IEnrollmentFileRepository
    {
        private readonly ProjectContext _dbContext;

        public EnrollmentFileRepository(ProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EnrollmentFile>> GetByEnrollmentIdAsync(Guid enrollmentId)
        {
            return await _dbContext.EnrollmentFiles
                .Where(f => f.EventEnrollmentId == enrollmentId && f.DeleteDate == null)
                .OrderBy(f => f.CreateDate)
                .ToListAsync();
        }

        public async Task<EnrollmentFile> CreateAsync(EnrollmentFile file)
        {
            file.Id = Guid.NewGuid();
            file.Create();

            _dbContext.EnrollmentFiles.Add(file);
            await _dbContext.SaveChangesAsync();

            return file;
        }

        public async Task<EnrollmentFile?> GetByIdAsync(Guid id)
        {
            return await _dbContext.EnrollmentFiles
                .Include(f => f.EventEnrollment)
                .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(f => f.Id == id && f.DeleteDate == null);
        }

        public async Task DeleteAsync(EnrollmentFile file)
        {
            file.Delete();
            await _dbContext.SaveChangesAsync();
        }
    }
}
