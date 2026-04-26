using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace ApiGestaoUsuarios.Infraestructure.Filters
{    
    public class AuditFilter : IActionFilter
    {        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var parameters = string.Join(", ", context.ActionArguments.Select(kv => $"{kv.Key}={kv.Value}"));
            Log.Information("Auditoria: Executando Ação: {action} em Controller: {controller} com Parâmetros: {parameters}",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name,
                parameters);
        }

        /// <summary>
        /// Executado após a action.
        /// Registra no log que a execução da action foi finalizada.
        /// </summary>
        /// <param name="context">Contexto da execução da action.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Log.Information("Auditoria: Finalizou {action} com o Status: {status}", context.ActionDescriptor.DisplayName, context.HttpContext.Response.StatusCode);
        }
    }
}
