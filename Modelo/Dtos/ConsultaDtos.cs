namespace Modelo.Dtos;

public class PeticionConsulta
{
    public string Texto { get; set; } = "";
}

public class RespuestaConsulta
{
    public string Enrutador { get; set; } = "";
    public string? Herramienta { get; set; }
    public Dictionary<string, string?> Argumentos { get; set; } = new();
    public string Resultado { get; set; } = "";
}