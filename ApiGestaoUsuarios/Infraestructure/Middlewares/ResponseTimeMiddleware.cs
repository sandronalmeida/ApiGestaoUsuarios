using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace ApiGestaoUsuarios.Infraestructure.Middlewares
{
    /// <summary>
    /// Middleware responsável por interceptar todas as respostas HTTP da aplicação
    /// e aplicar um "wrapper" padronizado em formato JSON.
    /// 
    /// Funcionalidades principais:
    /// - Mede o tempo de execução da requisição.
    /// - Loga informações básicas da requisição (método, rota, status, tempo).
    /// - Envolve a resposta original em um envelope JSON contendo metadados adicionais.
    /// - Mantém respostas não-JSON (como arquivos ou HTML) intactas.
    /// </summary>
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;        

        /// <summary>
        /// Construtor do middleware.
        /// </summary>
        /// <param name="next">Delegate para o próximo componente do pipeline.</param>
        /// <param name="logger">Logger para registrar informações de monitoramento.</param>
        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;            
        }
        /// <summary>
        /// Método principal do middleware que intercepta e processa cada requisição HTTP.
        /// </summary>
        /// <param name="context">Contexto da requisição HTTP.</param>
        /// <returns>Tarefa assíncrona representando a execução do middleware.</returns>
        public async Task InvokeAsync(HttpContext context)
        {            
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;

                // Executa o restante do pipeline
                await _next(context);

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                // Log de monitoramento básico
                Log.Information(
                    "Requisição {Method} {Path} finalizada em {Elapsed}ms com Status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMilliseconds,
                    context.Response.StatusCode
                );
                
                if (IsJsonResponse(context))
                {
                    context.Response.Body = originalBodyStream;

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var bodyText = await new StreamReader(memoryStream).ReadToEndAsync();

                    object? originalJson;
                    try
                    {
                        originalJson = JsonSerializer.Deserialize<object>(bodyText);
                    }
                    catch
                    {
                        originalJson = bodyText;
                    }

                    var responseEnvelope = new
                    {
                        dados_resposta = originalJson,
                        timestamp_resposta = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        tempo_da_resposta = $"{elapsedMilliseconds} ms",
                        success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                        statusCode = context.Response.StatusCode
                    };

                    var finalJson = JsonSerializer.Serialize(responseEnvelope);

                    // Importante: Limpar headers antes de reescrever se necessário
                    context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(finalJson);
                    await context.Response.WriteAsync(finalJson);
                }
                else
                {
                    // Se não for JSON (ex: um arquivo ou HTML), devolvemos o stream original intacto
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
            }
        }
        /// <summary>
        /// Verifica se a resposta atual é do tipo JSON.
        /// </summary>
        /// <param name="context">Contexto da requisição HTTP.</param>
        /// <returns><c>true</c> se o Content-Type for JSON; caso contrário, <c>false</c>.</returns>
        private static bool IsJsonResponse(HttpContext context)
        {
            return context.Response.ContentType != null &&
                   context.Response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
