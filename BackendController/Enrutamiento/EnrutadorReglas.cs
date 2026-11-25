using System.Globalization;
using System.Text.RegularExpressions;

namespace BackendController.Enrutamiento;

public class EnrutadorReglas : IEnrutador
{
    private static readonly HashSet<string> PalabrasVacias = new(StringComparer.OrdinalIgnoreCase)
    {
        "el", "la", "los", "las", "de", "del", "un", "una", "personaje", "personajes",
        "todos", "todas", "que", "se", "llamen", "llamado", "llamada", "serie", "saga", "pelicula", "película"
    };

    private readonly ILogger<EnrutadorReglas> _logger;

    public EnrutadorReglas(ILogger<EnrutadorReglas> logger)
    {
        _logger = logger;
    }

    public string Nombre => "Reglas";

    public Task<DecisionRuta?> DecidirAsync(string consulta, CancellationToken ct = default)
    {
        var texto = consulta.Trim().ToLowerInvariant();
        var argumentos = new Dictionary<string, object?>();

        // Serie: solo las que se pueden reconocer sin ambigüedad
        var serie = Regex.Match(texto, @"dragon\s*ball\s*(z|gt|super|daima|kai)\b");
        if (serie.Success)
            argumentos["serie"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(serie.Value);

        // Saga: lo que va después de la palabra "saga"
        var saga = Regex.Match(texto, @"saga\s+(?:de\s+|del\s+)?(\p{L}+)");
        if (saga.Success && !PalabrasVacias.Contains(saga.Groups[1].Value))
            argumentos["saga"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(saga.Groups[1].Value);

        string? herramienta = null;

        if (Regex.IsMatch(texto, @"m[aá]s poderos|m[aá]s fuerte|mayor poder|mayor nivel"))
        {
            herramienta = "personaje_mas_poderoso";
        }
        else if (Regex.IsMatch(texto, @"cu[aá]ntos|cu[aá]ntas|cuenta|cantidad|n[uú]mero de|total"))
        {
            herramienta = "contar_personajes";
            AñadirNombre(texto, argumentos);
        }
        else if (Regex.IsMatch(texto, @"busca|buscar|muestra|mu[eé]strame|dime|qui[eé]n|info|datos|lista"))
        {
            herramienta = "buscar_personajes";
            AñadirNombre(texto, argumentos);
        }

        if (herramienta is null)
        {
            _logger.LogInformation("Enrutador de reglas: sin coincidencia para '{Consulta}'", consulta);
            return Task.FromResult<DecisionRuta?>(null);
        }

        _logger.LogInformation("Enrutador de reglas: {Herramienta} para '{Consulta}'", herramienta, consulta);
        return Task.FromResult<DecisionRuta?>(new DecisionRuta(herramienta, argumentos));
    }

    private static void AñadirNombre(string texto, Dictionary<string, object?> argumentos)
    {
        var m = Regex.Match(texto, @"(?:busca(?:r)?|sobre|llamad[oa]s?|llamen|nombre|con|a)\s+(?:a\s+|al\s+)?(\p{L}+)");
        if (!m.Success) return;

        var palabra = m.Groups[1].Value;
        if (PalabrasVacias.Contains(palabra) || palabra.StartsWith("dragon")) return;

        argumentos["nombre"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(palabra);
    }
}