using System.Text;
using BackendController.Configuracion;
using BackendController.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositorio.Interfaces;
using Repositorio.RepositorioDataSet;
using Repositorio.RepositorioEF;
using BackendController.Enrutamiento;
using BackendController.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Patrón Options: cada sección del appsettings.json se asocia a su clase
builder.Services.Configure<OpcionesPersistencia>(
    builder.Configuration.GetSection(OpcionesPersistencia.Seccion));
builder.Services.Configure<OpcionesEnrutador>(
    builder.Configuration.GetSection(OpcionesEnrutador.Seccion));
builder.Services.Configure<OpcionesJwt>(
    builder.Configuration.GetSection(OpcionesJwt.Seccion));
builder.Services.Configure<OpcionesAutenticacion>(
    builder.Configuration.GetSection(OpcionesAutenticacion.Seccion));
builder.Services.Configure<OpcionesImportacion>(
    builder.Configuration.GetSection(OpcionesImportacion.Seccion));

// Entity Framework con MySQL
builder.Services.AddDbContext<ContextoDragonBall>((servicios, opciones) =>
{
    var cadena = servicios.GetRequiredService<IOptionsMonitor<OpcionesPersistencia>>()
        .CurrentValue.CadenaConexion;
    opciones.UseMySql(cadena, new MySqlServerVersion(new Version(8, 4, 0)));
});

// Las dos implementaciones de persistencia
builder.Services.AddSingleton<RepositorioPersonajesDataSet>();
builder.Services.AddScoped<RepositorioPersonajesEF>();
builder.Services.AddSingleton<SelectorPersistencia>();

// Los controladores piden la interfaz y reciben la implementación elegida
builder.Services.AddScoped<IRepositorioPersonajes>(
    servicios => servicios.GetRequiredService<SelectorPersistencia>().Obtener(servicios));

// Autenticación JWT
var jwt = builder.Configuration.GetSection(OpcionesJwt.Seccion).Get<OpcionesJwt>() ?? new OpcionesJwt();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Emisor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<ServicioToken>();
builder.Services.AddSingleton<ImportadorCsv>();


builder.Services.Configure<OpcionesLlm>(builder.Configuration.GetSection("Llm"));
builder.Services.Configure<OpcionesMcp>(builder.Configuration.GetSection("Mcp"));
builder.Services.AddSingleton<EnrutadorReglas>();
builder.Services.AddHttpClient<EnrutadorLlm>(c => c.Timeout = TimeSpan.FromSeconds(90));
builder.Services.AddScoped<ServicioConsultas>();


builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ContextoDragonBall>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // MySQL tarda unos segundos en estar listo, así que se reintenta
    for (var intento = 1; intento <= 10; intento++)
    {
        try
        {
            contexto.Database.Migrate();
            logger.LogInformation("Base de datos lista");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning("MySQL aún no responde (intento {Intento}): {Mensaje}", intento, ex.Message);
            Thread.Sleep(3000);
        }
    }
}
app.Run();