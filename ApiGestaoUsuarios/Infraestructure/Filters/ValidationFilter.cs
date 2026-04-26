using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGestaoUsuarios.Infraestructure.Filters
{
    /// <summary>
    /// Filtro de ação responsável por validar o estado do modelo (ModelState)
    /// antes da execução da action.
    /// 
    /// Funcionalidades principais:
    /// - Intercepta requisições antes da execução da action.
    /// - Verifica se o modelo recebido é válido.
    /// - Retorna uma resposta padronizada em caso de erros de validação.
    /// - Evita que a action seja executada quando os dados são inválidos.
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        /// <summary>
        /// Executado antes da action.
        /// Caso o <see cref="ModelState"/> seja inválido, interrompe o pipeline
        /// e retorna um erro 400 (Bad Request) com detalhes dos campos inválidos.
        /// </summary>
        /// <param name="context">Contexto da execução da action.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                context.Result = new BadRequestObjectResult(new
                {
                    message = "Erro de validação",
                    details = errors,
                    Success = false
                });
            }
        }

        /// <summary>
        /// Executado após a action.
        /// Neste filtro não há lógica implementada após a execução.
        /// </summary>
        /// <param name="context">Contexto da execução da action.</param>
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
