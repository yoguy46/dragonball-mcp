using BackendController.Configuracion;
using BackendController.Enrutamiento;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Modelo.Dtos;

namespace BackendController.Servicios;

public class OpcionesMcp
{
    public string Url { get; set; } = "http://localhost:5100/mcp";
}

public class ServicioConsultas
{
    private readonly IOptionsMonitor<OpcionesEnrutador> _opcionesEnrutador;
    private readonly IOptionsMonitor<OpcionesMcp> _opcionesMcp;
    private readonly EnrutadorReglas _reglas;
    private readonly EnrutadorLlm _llm;
    private readonly ILogger<ServicioConsultas> _logger;

    public ServicioConsultas(
        IOptionsMonitor<OpcionesEnrutador> opcionesEnrutador,
        IOptionsMonitor<OpcionesMcp> opcionesMcp,
        EnrutadorReglas reglas,
        EnrutadorLlm llm,
        ILogger<ServicioConsultas> logger)
    {
        _opcionesEnrutador = opcionesEnrutador;
        _opcionesMcp = opcionesMcp;
        _reglas = reglas;
        _llm = llm;
        _logger = logger;
    }

    public async Task<RespuestaConsulta> ConsultarAsync(string texto, CancellationToken ct = default)
    {
        var tipo = _opcionesEnrutador.CurrentValue.Tipo;
        var respuesta = new RespuestaConsulta();
        DecisionRuta? decision;

        try
        {
            if (string.Equals(tipo, "LLM", StringComparison.OrdinalIgnoreCase))
            {
                respuesta.Enrutador = "LLM";
                decision = await _llm.DecidirAsync(texto, ct);
            }
            else
            {
                // Primero reglas; si no reconocen la pregunta, se pasa al LLM
                respuesta.Enrutador = "Reglas";
                decision = await _reglas.DecidirAsync(texto, ct);
                if (decision is null)
                {
                    respuesta.Enrutador = "Reglas, y después LLM";
                    decision = await _llm.DecidirAsync(texto, ct);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo contactar con LM Studio");
            respuesta.Resultado = "No se pudo contactar con LM Studio. Comprueba que el servidor local está activo.";
            return respuesta;
        }

        if (decision is null)
        {
            respuesta.Resultado = "No he entendido la pregunta. Prueba a preguntar por personajes, sagas o series.";
            return respuesta;
        }

        respuesta.Herramienta = decision.Herramienta;
        respuesta.Argumentos = decision.Argumentos.ToDictionary(a => a.Key, a => a.Value?.ToString());

        var transporte = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(_opcionesMcp.CurrentValue.Url)
        });

        await using var cliente = await McpClient.CreateAsync(transporte, cancellationToken: ct);
        var resultado = await cliente.CallToolAsync(decision.Herramienta, decision.Argumentos, cancellationToken: ct);

        respuesta.Resultado = string.Join("\n", resultado.Content.OfType<TextContentBlock>().Select(b => b.Text));
        return respuesta;
    }
}