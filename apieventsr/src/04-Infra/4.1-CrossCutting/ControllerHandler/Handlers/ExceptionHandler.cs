using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace apieventsr.ControllerHandler.Handlers
{
    public static class ExceptionHandler
    {
        public async static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                KeyNotFoundException     => HttpStatusCode.NotFound,            // 404
                InvalidOperationException => HttpStatusCode.UnprocessableEntity, // 422 — regra de negócio violada
                _ => HttpStatusCode.InternalServerError,
            };

            await context.HttpErrorAnswer(statusCode, exception.Message);
        }

        private static Task HttpErrorAnswer(this HttpContext context, HttpStatusCode code, string message)
        {
            var result = JsonConvert.SerializeObject(new { error = message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}