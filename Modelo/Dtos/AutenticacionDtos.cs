namespace Modelo.Dtos;

public class PeticionLogin
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RespuestaLogin
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}
