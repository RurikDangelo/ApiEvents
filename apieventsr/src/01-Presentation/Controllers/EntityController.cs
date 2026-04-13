using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace apieventsr.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("Entity Actions")]
    public class EntityController : ControllerBase
    {
        private readonly ILogger<EntityController> _logger;
        private readonly IService _service;

        public EntityController(ILogger<EntityController> logger, IService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("{parametro}")]
        [SwaggerOperation("Get by id", "Retorna entidade baseado no id")]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(EntityResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Request Inválida")]
        public async Task<IActionResult> Get(int parametro)
        {
            var response = await _service.GetEntity(parametro);
            return Ok(response);
        }
    }
}