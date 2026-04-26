using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace ApiGestaoUsuarios.Infraestructure.Filters
{
    /// <summary>
    /// Filtro de auditoria que registra logs antes e depois da execução de uma action.
    /// Utiliza o Serilog para registrar informações de auditoria.
    /// </summary>
    public class AuditFilter : IActionFilter
    {
        /// <summary>
        /// Executado antes da execução da action.
        /// Registra no log o nome da action, o controller e os parâmetros recebidos.
        /// </summary>
        /// <param name="context">Contexto da execução da action, contendo informações sobre controller, action e parâmetros.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var parameters = string.Join(", ", context.ActionArguments.Select(kv => $"{kv.Key}={kv.Value}"));
            Log.Information("Auditoria: Executando Ação: {action} em Controller: {controller} com Parâmetros: {parameters}",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name,
                parameters);
        }

        /// <summary>
        /// Executado após a execução da action.
        /// Registra no log que a execução foi finalizada, incluindo o status da resposta HTTP.
        /// </summary>
        /// <param name="context">Contexto da execução da action, contendo informações sobre resultado e status da resposta.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Log.Information("Auditoria: Finalizou {action} com o Status: {status}", context.ActionDescriptor.DisplayName, context.HttpContext.Response.StatusCode);
        }
    }
}
