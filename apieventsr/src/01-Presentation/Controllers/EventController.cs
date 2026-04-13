using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace apieventsr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("Eventos")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;

        public EventController(ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        [HttpGet("all")]
        [SwaggerOperation("Listar todos os eventos", "Retorna eventos com status calculado e flag de inscrição da escola")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<EventListItemResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetAll()
        {
            // TODO: confirmar com o time o nome exato do claim de schoolId no JWT
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            Guid? schoolId = Guid.TryParse(schoolIdClaim, out var parsed) ? parsed : null;

            var response = await _eventService.GetAllAsync(schoolId);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation("Detalhe do evento", "Retorna informações completas, documentos, cronograma, premiação e flags de navegação")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(EventDetailResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Evento não encontrado")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // TODO: confirmar com o time o nome exato do claim de schoolId no JWT
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            Guid? schoolId = Guid.TryParse(schoolIdClaim, out var parsed) ? parsed : null;

            var response = await _eventService.GetByIdAsync(id, schoolId);
            return Ok(response);
        }
    }
}
