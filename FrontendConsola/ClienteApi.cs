using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Modelo.Dtos;
using Modelo.Entidades;

namespace FrontendConsola;

public class ClienteApi
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public ClienteApi(string urlBase)
    {
        _http = new HttpClient { BaseAddress = new Uri(urlBase), Timeout = TimeSpan.FromSeconds(120) };
    }

    public async Task LoginAsync(string usuario, string password)
    {
        var respuesta = await _http.PostAsJsonAsync("api/auth/login",
            new PeticionLogin { Usuario = usuario, Password = password });
        respuesta.EnsureSuccessStatusCode();

        var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaLogin>(Json);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", datos!.Token);
    }

    public async Task<List<Personaje>> ListarAsync(string? nombre = null, string? saga = null, string? serie = null)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(nombre)) partes.Add($"nombre={Uri.EscapeDataString(nombre)}");
        if (!string.IsNullOrWhiteSpace(saga)) partes.Add($"saga={Uri.EscapeDataString(saga)}");
        if (!string.IsNullOrWhiteSpace(serie)) partes.Add($"serie={Uri.EscapeDataString(serie)}");

        var url = "api/personajes" + (partes.Count > 0 ? "?" + string.Join("&", partes) : "");
        return await _http.GetFromJsonAsync<List<Personaje>>(url, Json) ?? new List<Personaje>();
    }

    public async Task<Personaje?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/personajes/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Personaje>(Json);
    }

    public async Task<Personaje?> CrearAsync(Personaje personaje)
    {
        var respuesta = await _http.PostAsJsonAsync("api/personajes", personaje, Json);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Personaje>(Json);
    }

    public async Task<bool> ActualizarAsync(Personaje personaje)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/personajes/{personaje.Id}", personaje, Json);
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        respuesta.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var respuesta = await _http.DeleteAsync($"api/personajes/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        respuesta.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<ResultadoImportacion?> ImportarAsync(string destino)
    {
        var respuesta = await _http.PostAsync($"api/importacion/{destino}", null);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResultadoImportacion>(Json);
    }

    public async Task<string> CambiarPersistenciaAsync(string modo)
    {
        var respuesta = await _http.PutAsync($"api/configuracion/persistencia/{modo}", null);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadAsStringAsync();
    }

    public async Task<RespuestaConsulta?> ConsultarAsync(string texto)
    {
        var respuesta = await _http.PostAsJsonAsync("api/consultas", new PeticionConsulta { Texto = texto }, Json);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<RespuestaConsulta>(Json);
    }
}