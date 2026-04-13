using Microsoft.AspNetCore.Http;

namespace apieventsr.ControllerHandler
{
    public class ControllerHandler
    {
        private readonly RequestDelegate _next;

        public ControllerHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await Handlers.ExceptionHandler.HandleExceptionAsync(context, ex);
            }
        }
    }
}