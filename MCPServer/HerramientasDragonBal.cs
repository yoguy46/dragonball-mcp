using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MCPServer;

[McpServerToolType]
public class HerramientasDragonBall
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ClienteApi _api;

    public HerramientasDragonBall(ClienteApi api)
    {
        _api = api;
    }

    [McpServerTool(Name = "buscar_personajes")]
    [Description("Busca personajes de Dragon Ball por nombre, saga/película o serie. Devuelve como máximo 10 resultados.")]
    public async Task<string> BuscarPersonajes(
        [Description("Parte del nombre del personaje, por ejemplo Goku")] string? nombre = null,
        [Description("Saga o película")] string? saga = null,
        [Description("Serie, por ejemplo Dragon Ball Z")] string? serie = null)
    {
        var lista = await _api.BuscarAsync(nombre, saga, serie);
        return JsonSerializer.Serialize(lista.Take(10), Json);
    }

    [McpServerTool(Name = "contar_personajes")]
    [Description("Cuenta cuántos personajes cumplen los filtros indicados.")]
    public async Task<string> ContarPersonajes(
        [Description("Parte del nombre")] string? nombre = null,
        [Description("Saga o película")] string? saga = null,
        [Description("Serie")] string? serie = null)
    {
        var lista = await _api.BuscarAsync(nombre, saga, serie);
        return JsonSerializer.Serialize(new { total = lista.Count }, Json);
    }

    [McpServerTool(Name = "personaje_mas_poderoso")]
    [Description("Devuelve el personaje con mayor nivel de poder, opcionalmente dentro de una saga o serie.")]
    public async Task<string> PersonajeMasPoderoso(
        [Description("Saga o película")] string? saga = null,
        [Description("Serie")] string? serie = null)
    {
        var lista = await _api.BuscarAsync(null, saga, serie);
        var mejor = lista.OrderByDescending(p => p.NivelDePoder).FirstOrDefault();
        return mejor is null
            ? JsonSerializer.Serialize(new { mensaje = "Sin resultados" }, Json)
            : JsonSerializer.Serialize(mejor, Json);
    }
}