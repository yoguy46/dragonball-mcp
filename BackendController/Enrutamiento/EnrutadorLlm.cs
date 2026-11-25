using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BackendController.Enrutamiento;

public class OpcionesLlm
{
    public string Url { get; set; } = "http://localhost:1234/v1";
    public string Modelo { get; set; } = "";
}

public class EnrutadorLlm : IEnrutador
{
    private static readonly HashSet<string> HerramientasValidas = new()
    {
        "buscar_personajes", "contar_personajes", "personaje_mas_poderoso"
    };

    private const string Instrucciones = """
        Eres un enrutador. Recibes una pregunta sobre personajes de Dragon Ball y debes elegir UNA herramienta.
        Herramientas disponibles:
        - buscar_personajes: argumentos opcionales nombre, saga, serie. Para listar o buscar personajes.
        - contar_personajes: argumentos opcionales nombre, saga, serie. Para saber cuántos personajes hay.
        - personaje_mas_poderoso: argumentos opcionales saga, serie (NO acepta nombre). Solo para saber quién es el más poderoso de todos, o de una saga o serie.
        - Si la pregunta menciona a un personaje concreto, usa buscar_personajes con su nombre.
        Responde SOLO con un JSON, sin explicaciones, con este formato:
        {"herramienta":"nombre_de_la_herramienta","argumentos":{"nombre":"...","saga":"...","serie":"..."}}
        Incluye en argumentos solo los datos que aparezcan en la pregunta.
        Si la pregunta no tiene nada que ver con los personajes, responde {"herramienta":"ninguna","argumentos":{}}.
        """;

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<OpcionesLlm> _opciones;
    private readonly ILogger<EnrutadorLlm> _logger;

    public EnrutadorLlm(HttpClient http, IOptionsMonitor<OpcionesLlm> opciones, ILogger<EnrutadorLlm> logger)
    {
        _http = http;
        _opciones = opciones;
        _logger = logger;
    }

    public string Nombre => "LLM";

    public async Task<DecisionRuta?> DecidirAsync(string consulta, CancellationToken ct = default)
    {
        var o = _opciones.CurrentValue;

        var cuerpo = new Dictionary<string, object?>
        {
            ["messages"] = new[]
            {
                new { role = "system", content = Instrucciones },
                new { role = "user", content = consulta }
            },
            ["temperature"] = 0
        };
        if (!string.IsNullOrWhiteSpace(o.Modelo)) cuerpo["model"] = o.Modelo;

        using var respuesta = await _http.PostAsJsonAsync($"{o.Url.TrimEnd('/')}/chat/completions", cuerpo, ct);
        respuesta.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
        var texto = doc.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString() ?? "";

        var inicio = texto.IndexOf('{');
        var fin = texto.LastIndexOf('}');
        if (inicio < 0 || fin <= inicio)
        {
            _logger.LogWarning("El LLM no devolvió JSON: {Texto}", texto);
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(texto[inicio..(fin + 1)]);
            var herramienta = json.RootElement.GetProperty("herramienta").GetString();
            if (herramienta is null || !HerramientasValidas.Contains(herramienta))
                return null;

            var argumentos = new Dictionary<string, object?>();
            if (json.RootElement.TryGetProperty("argumentos", out var args) && args.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in args.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(p.Value.GetString()))
                        argumentos[p.Name] = p.Value.GetString();
                }
            }

            _logger.LogInformation("Enrutador LLM: {Herramienta} para '{Consulta}'", herramienta, consulta);
            return new DecisionRuta(herramienta, argumentos);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON del LLM no válido: {Texto}", texto);
            return null;
        }
    }
}