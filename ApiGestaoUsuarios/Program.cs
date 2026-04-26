using ApiGestaoUsuarios.Domain.Interfaces;
using ApiGestaoUsuarios.Infraestructure.Data;
using ApiGestaoUsuarios.Infraestructure.Filters;
using ApiGestaoUsuarios.Infraestructure.Middlewares;
using ApiGestaoUsuarios.Infraestructure.Serialization;
using ApiGestaoUsuarios.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURAÇÃO DE LOGS (SERILOG)
// ============================================================================
// Configura o Serilog para logar em console e arquivo, com rotação diária.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ============================================================================
// CONFIGURAÇÃO DE CONTROLLERS E SERIALIZAÇÃO JSON
// ============================================================================
// Configuração global de JSON com System.Text.Json
builder.Services.AddControllers( options =>
                                  {
                                  // Adiciona filtros globais para todos os endpoints   
                                   options.Filters.Add<ValidationFilter>();
                                   options.Filters.Add<AuditFilter>();                                  
                                  })
                                  .AddJsonOptions(options =>
                                   {
                                  // Ignorar valores nulos
                                     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

// snake_case nos nomes das propriedades
 options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();

// Formato de data customizado
   options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
});

// ============================================================================
// PERSISTÊNCIA DE DADOS (ENTITY FRAMEWORK)
// ============================================================================
// Configuração do DbContext utilizando SQLite.
// Add services to the container.
builder.Services.AddDbContext<ApiGestaoUsuariosDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================================
// INJEÇÃO DE DEPENDÊNCIA (DI)
// ============================================================================
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
// Hashing
builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddScoped<IPasswordService, PasswordService>();


// ============================================================================
// CONFIGURAÇÃO DE SEGURANÇA (CORS)
// ============================================================================
// Define as origens permitidas para consumo da API (Frontend).
// 1. Lendo as configurações do CORS do appsettings.json
var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigin"]
                ?? "https://brunotrbr.github.io"; // Fallback de segurança

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================================================
// DOCUMENTAÇÃO (SWAGGER)
// ============================================================================
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================================================
// PIPELINE DE REQUISIÇÕES (MIDDLEWARES)
// ============================================================================
// Configure the HTTP request pipeline.

// Ativa a interface gráfica do Swagger apenas em ambiente de desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middlewares customizados (A ordem é crucial para o funcionamento).
// 1. Captura de exceções globais para retornos padronizados.
app.UseMiddleware<ExceptionHandlingMiddleware>(); // primeiro: captura exceções globais
// 2. Envelopamento de resposta e medição de performance.
app.UseMiddleware<ResponseWrapperMiddleware>();      // mede tempo de resposta

app.UseHttpsRedirection();

// Aplica a política de CORS definida anteriormente.
app.UseCors("FrontendPolicy");

app.UseAuthorization();

// Mapeamento automático dos endpoints dos Controllers.
app.MapControllers();

app.Run();
