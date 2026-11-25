namespace Modelo.Dtos;

public class ResultadoImportacion
{
    public string Destino { get; set; } = string.Empty;
    public int Importados { get; set; }
    public int Omitidos { get; set; }
}