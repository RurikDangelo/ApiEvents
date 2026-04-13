using apieventsr.Application.Dtos.Requests;
using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using apieventsr.Data.Interfaces;
using apieventsr.Domain.Entities;
using apieventsr.Domain.Enums;

namespace apieventsr.Application.Services
{
    public class EventEnrollmentService : IEventEnrollmentService
    {
        private readonly IEventEnrollmentRepository _repository;

        public EventEnrollmentService(IEventEnrollmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SegmentAvailabilityResponse>> GetAvailableSegmentsAsync(
            Guid eventId, Guid? schoolId, Guid? userId, bool filterByUser)
        {
            // 1. Busca segmentos (com filtro de perfil se necessário)
            var segments = await _repository.GetSegmentsWithCategoriesAsync(userId, filterByUser);

            // 2. Busca quais combinações segmento+categoria já estão ocupadas pela escola
            var enrolledPairs = schoolId.HasValue
                ? await _repository.GetEnrolledPairsAsync(eventId, schoolId.Value)
                : new HashSet<(Guid, Guid)>();

            // 3. Monta a resposta marcando disponibilidade de cada categoria
            return segments
                .Select(s => new SegmentAvailabilityResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Categories = s.Categories
                        .OrderBy(c => c.Name)
                        .Select(c => new CategoryAvailabilityItem
                        {
                            Id = c.Id,
                            Name = c.Name,
                            // Disponível = a escola ainda não inscreveu neste segmento+categoria
                            IsAvailable = !enrolledPairs.Contains((s.Id, c.Id))
                        })
                        .ToList()
                })
                .ToList();
        }

        public async Task<List<EnrollmentListItemResponse>> GetAllAsync(
            Guid schoolId, Guid? userId, string role)
        {
            var enrollments = await _repository.GetAllBySchoolAsync(schoolId);

            // Coordenador: filtra pelas inscrições dos segmentos que gerencia
            if (role.Equals("coordinator", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                var allowedSegmentIds = await _repository.GetUserSegmentIdsAsync(userId.Value);
                enrollments = enrollments
                    .Where(e => allowedSegmentIds.Contains(e.SegmentId))
                    .ToList();
            }

            // Professor: vê apenas as próprias inscrições (onde é autor)
            if (role.Equals("professor", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                enrollments = enrollments
                    .Where(e => e.AuthorId == userId.Value)
                    .ToList();
            }

            return enrollments.Select(e =>
            {
                var status = CalculateEventStatus(e.Event);
                var isClosed = status == EventStatus.Closed;

                return new EnrollmentListItemResponse
                {
                    Id           = e.Id,
                    ProjectName  = e.ProjectName,
                    EventTitle   = e.Event.Title,
                    SegmentName  = e.Segment.Name,
                    CategoryName = e.Category.Name,
                    EventStatus  = status,
                    CreatedAt    = e.CreateDate,
                    // Ações só são permitidas enquanto o evento não está encerrado
                    CanEdit   = !isClosed,
                    CanDelete = !isClosed,
                    CanUpload = !isClosed
                };
            }).ToList();
        }

        // Calcula o status do evento a partir das datas (mesma lógica do EventService)
        private static EventStatus CalculateEventStatus(Domain.Entities.Event e)
        {
            var now = DateTime.UtcNow;
            if (now < e.EnrollmentStartDate) return EventStatus.ComingSoon;
            if (now <= e.EnrollmentEndDate)  return EventStatus.EnrollmentOpen;
            if (now < e.ResultDate)          return EventStatus.InProgress;
            return EventStatus.Closed;
        }

        public async Task<EnrollmentCreatedResponse> UpdateAsync(
            Guid id, UpdateEnrollmentRequest request, Guid schoolId, Guid? userId, string role)
        {
            // 1. Inscrição existe e pertence à escola?
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment is null)
                throw new KeyNotFoundException($"Inscrição {id} não encontrada.");

            if (enrollment.SchoolId != schoolId)
                throw new InvalidOperationException("Você não tem permissão para editar esta inscrição.");

            // 2. Evento ainda permite edição?
            var status = CalculateEventStatus(enrollment.Event);
            if (status == EventStatus.Closed)
                throw new InvalidOperationException("Não é possível editar inscrições de eventos encerrados.");

            // 3. Validação de perfil
            if (role.Equals("coordinator", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                var allowedSegmentIds = await _repository.GetUserSegmentIdsAsync(userId.Value);
                if (!allowedSegmentIds.Contains(enrollment.SegmentId))
                    throw new InvalidOperationException("Você não gerencia o segmento desta inscrição.");
            }

            if (role.Equals("professor", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                if (enrollment.AuthorId != userId.Value)
                    throw new InvalidOperationException("Professor só pode editar inscrições que ele mesmo criou.");
            }

            // 4. Aplica as alterações
            enrollment.ProjectName             = request.ProjectName;
            enrollment.ResponsibleName         = request.ResponsibleName;
            enrollment.ManagementRepresentative = request.ManagementRepresentative;

            var updated = await _repository.UpdateAsync(enrollment);

            return new EnrollmentCreatedResponse
            {
                Id           = updated.Id,
                ProjectName  = updated.ProjectName,
                SegmentName  = updated.Segment.Name,
                CategoryName = updated.Category.Name,
                CreatedAt    = updated.CreateDate
            };
        }

        public async Task DeleteAsync(Guid id, Guid schoolId, Guid? userId, string role)
        {
            // 1. Inscrição existe e pertence à escola?
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment is null)
                throw new KeyNotFoundException($"Inscrição {id} não encontrada.");

            if (enrollment.SchoolId != schoolId)
                throw new InvalidOperationException("Você não tem permissão para excluir esta inscrição.");

            // 2. Evento ainda permite exclusão?
            var status = CalculateEventStatus(enrollment.Event);
            if (status == EventStatus.Closed)
                throw new InvalidOperationException("Não é possível excluir inscrições de eventos encerrados.");

            // 3. Validação de perfil
            if (role.Equals("coordinator", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                var allowedSegmentIds = await _repository.GetUserSegmentIdsAsync(userId.Value);
                if (!allowedSegmentIds.Contains(enrollment.SegmentId))
                    throw new InvalidOperationException("Você não gerencia o segmento desta inscrição.");
            }

            if (role.Equals("professor", StringComparison.OrdinalIgnoreCase) && userId.HasValue)
            {
                if (enrollment.AuthorId != userId.Value)
                    throw new InvalidOperationException("Professor só pode excluir inscrições que ele mesmo criou.");
            }

            await _repository.DeleteAsync(enrollment);
        }

        public async Task<EnrollmentCreatedResponse> CreateAsync(
            CreateEnrollmentRequest request, Guid schoolId, Guid authorId)
        {
            // 1. Evento existe e está com inscrições abertas?
            var evento = await _repository.GetEventByIdAsync(request.EventId);
            if (evento is null)
                throw new KeyNotFoundException($"Evento {request.EventId} não encontrado.");

            var now = DateTime.UtcNow;
            var isEnrollmentOpen = now >= evento.EnrollmentStartDate && now <= evento.EnrollmentEndDate;
            if (!isEnrollmentOpen)
                throw new InvalidOperationException("Este evento não está com inscrições abertas no momento.");

            // 2. Segmento e categoria existem, estão ativos e pertencem um ao outro?
            var (segment, category) = await _repository.GetActiveSegmentCategoryAsync(
                request.SegmentId, request.CategoryId);

            if (segment is null)
                throw new KeyNotFoundException($"Segmento {request.SegmentId} não encontrado ou inativo.");

            if (category is null)
                throw new KeyNotFoundException(
                    $"Categoria {request.CategoryId} não encontrada, inativa ou não pertence ao segmento informado.");

            // 3. Escola já tem inscrição ativa neste evento+segmento+categoria?
            var alreadyEnrolled = await _repository.EnrollmentExistsAsync(
                request.EventId, schoolId, request.SegmentId, request.CategoryId);

            if (alreadyEnrolled)
                throw new InvalidOperationException(
                    "Já existe uma inscrição ativa desta escola neste evento para o segmento e categoria informados.");

            // 4. Tudo válido — persiste
            var enrollment = new EventEnrollment
            {
                ProjectName             = request.ProjectName,
                ResponsibleName         = request.ResponsibleName,
                ManagementRepresentative = request.ManagementRepresentative,
                AuthorId                = authorId,
                SchoolId                = schoolId,
                EventId                 = request.EventId,
                SegmentId               = request.SegmentId,
                CategoryId              = request.CategoryId
            };

            var created = await _repository.CreateAsync(enrollment);

            return new EnrollmentCreatedResponse
            {
                Id           = created.Id,
                ProjectName  = created.ProjectName,
                SegmentName  = segment.Name,
                CategoryName = category.Name,
                CreatedAt    = created.CreateDate
            };
        }
    }
}
