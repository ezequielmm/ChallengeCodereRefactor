using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Challenge.Domain.Repositories.Interfaces;
using Challenge.Application.Interfaces;
using Challenge.Application.Services;
using Challenge.Infrastructure.Persistence;
using Challenge.Infrastructure.Data;

[assembly: InternalsVisibleTo("Challenge.Tests")]

var builder = WebApplication.CreateBuilder(args);

// Agrega servicios al contenedor
Program.ConfigureDatabase(builder.Services, builder.Configuration);

// Registra los servicios y repositorios necesarios
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddScoped<IShowRepository, ShowRepository>();

// Registra HttpClient para realizar llamadas HTTP externas
builder.Services.AddHttpClient();

// Configura los controladores y la documentación de la API
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Clase parcial para permitir pruebas unitarias.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Configura el contexto de la base de datos con la cadena de conexión proporcionada.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }
            options.UseSqlServer(connectionString);
        });
    }

    /// <summary>
    /// Método dummy para pruebas unitarias.
    /// </summary>
    public void Dummy()
    {
        Console.WriteLine("This is for unit testing");
    }
}
