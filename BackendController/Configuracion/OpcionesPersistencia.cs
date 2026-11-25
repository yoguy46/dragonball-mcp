namespace BackendController.Configuracion;

public class OpcionesPersistencia
{
    public const string Seccion = "Persistencia";

    public string Modo { get; set; } = "EF";
    public string CadenaConexion { get; set; } = string.Empty;
}