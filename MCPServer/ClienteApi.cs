using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Modelo.Dtos;
using Modelo.Entidades;

namespace MCPServer;

public class OpcionesApi
{
    public string UrlBase { get; set; } = "http://localhost:5000";
    public string Usuario { get; set; } = "";
    public string Password { get; set; } = "";
}

public class ClienteApi
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<OpcionesApi> _opciones;
    private readonly ILogger<ClienteApi> _logger;
    private string? _token;

    public ClienteApi(HttpClient http, IOptionsMonitor<OpcionesApi> opciones, ILogger<ClienteApi> logger)
    {
        _http = http;
        _opciones = opciones;
        _logger = logger;
    }

    private async Task LoginAsync()
    {
        var o = _opciones.CurrentValue;
        var respuesta = await _http.PostAsJsonAsync(
            $"{o.UrlBase}/api/auth/login",
            new PeticionLogin { Usuario = o.Usuario, Password = o.Password });
        respuesta.EnsureSuccessStatusCode();

        var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaLogin>(Json);
        _token = datos!.Token;
        _logger.LogInformation("MCP autenticado en la API");
    }

    public async Task<List<Personaje>> BuscarAsync(string? nombre = null, string? saga = null, string? serie = null)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(nombre)) partes.Add($"nombre={Uri.EscapeDataString(nombre)}");
        if (!string.IsNullOrWhiteSpace(saga)) partes.Add($"saga={Uri.EscapeDataString(saga)}");
        if (!string.IsNullOrWhiteSpace(serie)) partes.Add($"serie={Uri.EscapeDataString(serie)}");

        var url = $"{_opciones.CurrentValue.UrlBase}/api/personajes";
        if (partes.Count > 0) url += "?" + string.Join("&", partes);

        for (var intento = 0; intento < 2; intento++)
        {
            if (_token is null) await LoginAsync();

            using var peticion = new HttpRequestMessage(HttpMethod.Get, url);
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            using var respuesta = await _http.SendAsync(peticion);

            if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _token = null;
                continue;
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<List<Personaje>>(Json) ?? new List<Personaje>();
        }

        throw new InvalidOperationException("No se pudo autenticar contra la API");
    }
}