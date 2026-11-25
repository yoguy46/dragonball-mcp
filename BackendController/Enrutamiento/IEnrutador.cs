namespace BackendController.Enrutamiento;

public record DecisionRuta(string Herramienta, Dictionary<string, object?> Argumentos);

public interface IEnrutador
{
    string Nombre { get; }

    // Devuelve la herramienta a llamar y sus argumentos, o null si no sabe qué hacer
    Task<DecisionRuta?> DecidirAsync(string consulta, CancellationToken ct = default);
}