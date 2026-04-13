using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace apieventsr.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ProjectContext _dbContext;

        public EventRepository(ProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Retorna todos os eventos ativos, ordenados do mais recente para o mais antigo
        public async Task<List<Event>> GetAllAsync(Guid? schoolId)
        {
            return await _dbContext.Events
                .Where(e => e.DeleteDate == null)
                .OrderByDescending(e => e.EnrollmentStartDate)
                .ToListAsync();
        }

        // Retorna o evento pelo ID com seus documentos (para o detalhe)
        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Events
                .Include(e => e.Documents.Where(d => d.DeleteDate == null))
                .FirstOrDefaultAsync(e => e.Id == id && e.DeleteDate == null);
        }

        // Retorna os IDs dos eventos nos quais a escola já tem inscrição ativa
        public async Task<List<Guid>> GetEnrolledEventIdsBySchoolAsync(Guid schoolId, List<Guid> eventIds)
        {
            return await _dbContext.EventEnrollments
                .Where(en => en.SchoolId == schoolId
                          && en.DeleteDate == null
                          && eventIds.Contains(en.EventId))
                .Select(en => en.EventId)
                .Distinct()
                .ToListAsync();
        }
    }
}
