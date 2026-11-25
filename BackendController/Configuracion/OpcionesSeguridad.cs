namespace BackendController.Configuracion;

public class OpcionesJwt
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 60;
}

public class UsuarioConfigurado
{
    public string Nombre { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class OpcionesAutenticacion
{
    public const string Seccion = "Autenticacion";

    public List<UsuarioConfigurado> Usuarios { get; set; } = new();
}
