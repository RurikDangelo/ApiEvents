using apieventsr.Application.Dtos.Requests;
using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace apieventsr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/event-enrollment")]
    [Tags("Inscrições")]
    public class EventEnrollmentController : ControllerBase
    {
        private readonly IEventEnrollmentService _service;

        public EventEnrollmentController(IEventEnrollmentService service)
        {
            _service = service;
        }

        private string? GetUserIdClaim()
        {
            return User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet("all")]
        [SwaggerOperation(
            "Listar minhas inscrições",
            "Retorna as inscrições da escola com flags de permissão calculadas no servidor. " +
            "Escola vê todas; Coordenador vê apenas as de seus segmentos; Professor vê apenas as próprias.")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<EnrollmentListItemResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetAll()
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            var userIdClaim = GetUserIdClaim();
            Guid? userId = Guid.TryParse(userIdClaim, out var parsedUser) ? parsedUser : null;

            // TODO: confirmar com o time o claim e os valores de role no Keycloak
            var role = User.FindFirst("role")?.Value ?? "school";

            var result = await _service.GetAllAsync(schoolId, userId, role);
            return Ok(result);
        }

        [HttpGet("segment/{eventId:guid}")]
        [SwaggerOperation(
            "Segmentos e categorias disponíveis",
            "Retorna os segmentos e categorias para inscrição no evento. " +
            "Escola vê todos; Coordenador e Professor veem apenas os segmentos vinculados. " +
            "IsAvailable=false indica que a escola já tem inscrição ativa naquela combinação.")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<SegmentAvailabilityResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetAvailableSegments(Guid eventId)
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            Guid? schoolId = Guid.TryParse(schoolIdClaim, out var parsedSchool) ? parsedSchool : null;

            var userIdClaim = GetUserIdClaim();
            Guid? userId = Guid.TryParse(userIdClaim, out var parsedUser) ? parsedUser : null;

            // TODO: confirmar com o time o claim e os valores de role no Keycloak
            // Escola → vê todos os segmentos (filterByUser = false)
            // Coordenador ou Professor → vê apenas os segmentos vinculados (filterByUser = true)
            var roleClaim = User.FindFirst("role")?.Value ?? string.Empty;
            var filterByUser = roleClaim.Equals("coordinator", StringComparison.OrdinalIgnoreCase)
                            || roleClaim.Equals("professor", StringComparison.OrdinalIgnoreCase);

            var result = await _service.GetAvailableSegmentsAsync(eventId, schoolId, userId, filterByUser);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            "Excluir inscrição",
            "Exclusão lógica (seta DeleteDate). Retorna 204 sem corpo. " +
            "Retorna 422 se o evento estiver encerrado ou se o perfil não tiver permissão.")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Inscrição não encontrada")]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada ou sem permissão")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            var userIdClaim = GetUserIdClaim();
            Guid? userId = Guid.TryParse(userIdClaim, out var parsedUser) ? parsedUser : null;

            var role = User.FindFirst("role")?.Value ?? "school";

            await _service.DeleteAsync(id, schoolId, userId, role);
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            "Editar inscrição",
            "Atualiza o nome do projeto, responsável e representante da gestão. " +
            "Não é possível alterar evento, segmento ou categoria após a criação. " +
            "Retorna 422 se o evento estiver encerrado ou se o perfil não tiver permissão.")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(EnrollmentCreatedResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Inscrição não encontrada")]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada ou sem permissão")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnrollmentRequest request)
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            var userIdClaim = GetUserIdClaim();
            Guid? userId = Guid.TryParse(userIdClaim, out var parsedUser) ? parsedUser : null;

            var role = User.FindFirst("role")?.Value ?? "school";

            var result = await _service.UpdateAsync(id, request, schoolId, userId, role);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            "Criar inscrição",
            "Inscreve a escola em um evento para um segmento e categoria específicos. " +
            "schoolId e authorId são extraídos automaticamente do token JWT. " +
            "Retorna 422 se o evento não estiver com inscrições abertas ou se já houver inscrição ativa.")]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(EnrollmentCreatedResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Evento, segmento ou categoria não encontrado")]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            var userIdClaim = GetUserIdClaim();
            if (!Guid.TryParse(userIdClaim, out var authorId))
                return Unauthorized(new { error = "Claim sub ausente ou inválido no token." });

            var result = await _service.CreateAsync(request, schoolId, authorId);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
    }
}
