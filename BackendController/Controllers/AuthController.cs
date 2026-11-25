using BackendController.Configuracion;
using BackendController.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Modelo.Dtos;

namespace BackendController.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IOptionsSnapshot<OpcionesAutenticacion> _opciones;
    private readonly ServicioToken _servicioToken;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IOptionsSnapshot<OpcionesAutenticacion> opciones,
        ServicioToken servicioToken,
        ILogger<AuthController> logger)
    {
        _opciones = opciones;
        _servicioToken = servicioToken;
        _logger = logger;
    }

    [HttpPost("login")]
    public ActionResult<RespuestaLogin> Login([FromBody] PeticionLogin peticion)
    {
        var valido = _opciones.Value.Usuarios.Any(u =>
            u.Nombre == peticion.Usuario && u.Password == peticion.Password);

        if (!valido)
        {
            _logger.LogWarning("Intento de login fallido para el usuario {Usuario}", peticion.Usuario);
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
 






















       }

        _logger.LogInformation("Login correcto del usuario {Usuario}", peticion.Usuario);
        return Ok(_servicioToken.Generar(peticion.Usuario));
    }
}
