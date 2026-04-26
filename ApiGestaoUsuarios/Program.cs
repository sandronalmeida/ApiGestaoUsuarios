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

//configura o Serilog para logar em console e arquivo, com rotação diária
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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


// Add services to the container.
builder.Services.AddDbContext<ApiGestaoUsuariosDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de dependência
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
// Hashing
builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddScoped<IPasswordService, PasswordService>();


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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ordem dos middlewares importa
app.UseMiddleware<ExceptionHandlingMiddleware>(); // primeiro: captura exceções globais
app.UseMiddleware<ResponseWrapperMiddleware>();      // mede tempo de resposta

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
