using System.Net;
using System.Text.Json;

namespace LocadoraVeiculos.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro nao tratado na requisicao {Metodo} {Caminho}",
                    context.Request.Method, context.Request.Path);

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var resposta = new
                {
                    status = 500,
                    mensagem = "Ocorreu um erro interno no servidor.",
                    detalhe = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(resposta));
            }
        }
    }
}
