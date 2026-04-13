using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace apieventsr.Data.Repositories
{
    public class EventEnrollmentRepository : IEventEnrollmentRepository
    {
        private readonly ProjectContext _dbContext;

        public EventEnrollmentRepository(ProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Retorna segmentos ativos com categorias ativas.
        // Se filterByUser=true, limita aos segmentos que o usuário gerencia (UserSegment)
        public async Task<List<Segment>> GetSegmentsWithCategoriesAsync(Guid? userId, bool filterByUser)
        {
            var query = _dbContext.Segments
                .Where(s => s.IsActive && s.DeleteDate == null)
                .Include(s => s.Categories.Where(c => c.IsActive && c.DeleteDate == null));

            if (filterByUser && userId.HasValue)
            {
                var allowedSegmentIds = await _dbContext.UserSegments
                    .Where(us => us.UserId == userId.Value && us.DeleteDate == null)
                    .Select(us => us.SegmentId)
                    .ToListAsync();

                return await query
                    .Where(s => allowedSegmentIds.Contains(s.Id))
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }

            return await query
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // Retorna os pares (SegmentId, CategoryId) nos quais a escola já está inscrita neste evento
        public async Task<HashSet<(Guid SegmentId, Guid CategoryId)>> GetEnrolledPairsAsync(Guid eventId, Guid schoolId)
        {
            var pairs = await _dbContext.EventEnrollments
                .Where(e => e.EventId == eventId
                         && e.SchoolId == schoolId
                         && e.DeleteDate == null)
                .Select(e => new { e.SegmentId, e.CategoryId })
                .ToListAsync();

            return pairs.Select(p => (p.SegmentId, p.CategoryId)).ToHashSet();
        }

        // Retorna o evento ativo pelo ID
        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            return await _dbContext.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.DeleteDate == null);
        }

        // Verifica se segmento e categoria existem, estão ativos e o segment é pai da category
        public async Task<(Segment? Segment, Category? Category)> GetActiveSegmentCategoryAsync(Guid segmentId, Guid categoryId)
        {
            var segment = await _dbContext.Segments
                .FirstOrDefaultAsync(s => s.Id == segmentId && s.IsActive && s.DeleteDate == null);

            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId
                                       && c.SegmentId == segmentId
                                       && c.IsActive
                                       && c.DeleteDate == null);

            return (segment, category);
        }

        // Verifica duplicidade antes de tentar inserir (retorna mensagem mais clara que a exception do banco)
        public async Task<bool> EnrollmentExistsAsync(Guid eventId, Guid schoolId, Guid segmentId, Guid categoryId)
        {
            return await _dbContext.EventEnrollments
                .AnyAsync(e => e.EventId == eventId
                            && e.SchoolId == schoolId
                            && e.SegmentId == segmentId
                            && e.CategoryId == categoryId
                            && e.DeleteDate == null);
        }

        // Retorna todas as inscrições ativas da escola com dados do evento, segmento e categoria
        public async Task<List<EventEnrollment>> GetAllBySchoolAsync(Guid schoolId)
        {
            return await _dbContext.EventEnrollments
                .Where(e => e.SchoolId == schoolId && e.DeleteDate == null)
                .Include(e => e.Event)
                .Include(e => e.Segment)
                .Include(e => e.Category)
                .OrderByDescending(e => e.CreateDate)
                .ToListAsync();
        }

        // Retorna IDs dos segmentos que o usuário gerencia (usado para filtro de Coordenador)
        public async Task<List<Guid>> GetUserSegmentIdsAsync(Guid userId)
        {
            return await _dbContext.UserSegments
                .Where(us => us.UserId == userId && us.DeleteDate == null)
                .Select(us => us.SegmentId)
                .ToListAsync();
        }

        // Retorna uma inscrição pelo ID com dados de Event, Segment e Category
        public async Task<EventEnrollment?> GetByIdAsync(Guid id)
        {
            return await _dbContext.EventEnrollments
                .Include(e => e.Event)
                .Include(e => e.Segment)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.DeleteDate == null);
        }

        // Atualiza os campos editáveis da inscrição
        public async Task<EventEnrollment> UpdateAsync(EventEnrollment enrollment)
        {
            enrollment.Update();
            await _dbContext.SaveChangesAsync();
            return enrollment;
        }

        // Exclusão lógica: seta DeleteDate e salva
        public async Task DeleteAsync(EventEnrollment enrollment)
        {
            enrollment.Delete();
            await _dbContext.SaveChangesAsync();
        }

        // Persiste a nova inscrição no banco
        public async Task<EventEnrollment> CreateAsync(EventEnrollment enrollment)
        {
            enrollment.Id = Guid.NewGuid();
            enrollment.Create();

            _dbContext.EventEnrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();

            return enrollment;
        }
    }
}
