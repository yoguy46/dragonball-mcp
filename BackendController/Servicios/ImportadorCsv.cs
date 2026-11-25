using System.Globalization;
using BackendController.Configuracion;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Modelo.Entidades;

namespace BackendController.Servicios;

public class ImportadorCsv
{
    private readonly IOptionsMonitor<OpcionesImportacion> _opciones;
    private readonly ILogger<ImportadorCsv> _logger;

    public ImportadorCsv(IOptionsMonitor<OpcionesImportacion> opciones, ILogger<ImportadorCsv> logger)
    {
        _opciones = opciones;
        _logger = logger;
    }

    public Task<(List<Personaje> Personajes, int Omitidos)> LeerAsync()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, _opciones.CurrentValue.RutaCsv);
        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No se encuentra el fichero CSV en {ruta}");

        return Task.Run(() => Leer(ruta));
    }

    private (List<Personaje> Personajes, int Omitidos) Leer(string ruta)
    {
        var personajes = new List<Personaje>();
        var omitidos = 0;

        using var lector = new TextFieldParser(ruta);
        lector.TextFieldType = FieldType.Delimited;
        lector.SetDelimiters(",");
        lector.HasFieldsEnclosedInQuotes = true;

        if (!lector.EndOfData) lector.ReadFields();

        while (!lector.EndOfData)
        {
            string[]? campos;
            try
            {
                campos = lector.ReadFields();
            }
            catch (MalformedLineException)
            {
                omitidos++;
                continue;
            }

            if (campos is null || campos.Length < 4)
            {
                omitidos++;
                continue;
            }

            var nombre = campos[0].Trim();
            var textoPoder = campos[1].Replace(",", "").Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                !decimal.TryParse(textoPoder, NumberStyles.Number, CultureInfo.InvariantCulture, out var poder))
            {
                omitidos++;
                continue;
            }

            personajes.Add(new Personaje
            {
                Nombre = nombre,
                NivelDePoder = poder,
                SagaOPelicula = campos[2].Trim(),
                Serie = campos[3].Trim()
            });
        }

        _logger.LogInformation("CSV leído: {Validos} filas válidas y {Omitidos} omitidas", personajes.Count, omitidos);
        return (personajes, omitidos);
    }
}