using apieventsr.Application.Dtos.Responses;
using apieventsr.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace apieventsr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/event-enrollment/{enrollmentId:guid}/files")]
    [Tags("Arquivos de Inscrição")]
    public class EnrollmentFileController : ControllerBase
    {
        private readonly IEnrollmentFileService _service;

        public EnrollmentFileController(IEnrollmentFileService service)
        {
            _service = service;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            "Upload de arquivo",
            "Envia um arquivo para a inscrição. Limites: 6 total, 4 imagens, 1 PDF, 1 vídeo. " +
            "Tipos aceitos: JPEG, PNG, GIF, WEBP, PDF, MP4, WEBM, MOV. " +
            "Retorna 422 se qualquer limite ou regra for violada.")]
        [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(UploadFileResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Inscrição não encontrada")]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Limite ou tipo inválido")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> Upload(Guid enrollmentId, IFormFile file)
        {
            // TODO: confirmar com o time os nomes exatos dos claims no JWT do Keycloak
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            using var stream = file.OpenReadStream();
            var result = await _service.UploadAsync(
                enrollmentId, stream, file.FileName, file.ContentType, file.Length, schoolId);
            return CreatedAtAction(nameof(Upload), new { enrollmentId }, result);
        }

        [HttpDelete("{fileId:guid}")]
        [SwaggerOperation(
            "Remover arquivo",
            "Exclusão lógica no banco + remoção física do disco. Retorna 204 sem corpo.")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Arquivo não encontrado")]
        [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Sem permissão")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
        public async Task<IActionResult> Delete(Guid enrollmentId, Guid fileId)
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!Guid.TryParse(schoolIdClaim, out var schoolId))
                return Unauthorized(new { error = "Claim school_id ausente ou inválido no token." });

            await _service.DeleteAsync(enrollmentId, fileId, schoolId);
            return NoContent();
        }
    }
}
