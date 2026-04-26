using Serilog;
using System.Net;
using System.Text.Json;

namespace ApiGestaoUsuarios.Infraestructure.Middlewares
{
    /// <summary>
    /// Middleware responsável por capturar exceções não tratadas durante o processamento das requisições
    /// e retornar uma resposta padronizada em formato JSON.
    /// 
    /// Funcionalidades principais:
    /// - Intercepta exceções lançadas em qualquer parte do pipeline.
    /// - Registra logs detalhados do erro para monitoramento e auditoria.
    /// - Retorna uma resposta consistente com status HTTP 500 (Internal Server Error).
    /// - Evita que detalhes sensíveis da exceção sejam expostos ao cliente.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;        

        /// <summary>
        /// Construtor do middleware.
        /// </summary>
        /// <param name="next">Delegate que representa o próximo componente do pipeline.</param>
        /// <param name="logger">Instância de logger para registrar informações sobre exceções.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;            
        }

        /// <summary>
        /// Método principal do middleware que intercepta requisições,
        /// captura exceções não tratadas e retorna uma resposta padronizada.
        /// </summary>
        /// <param name="context">Contexto da requisição HTTP.</param>
        /// <returns>Tarefa assíncrona representando a execução do middleware.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var msg = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, msg);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new { message = msg };
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        }
    }
}
