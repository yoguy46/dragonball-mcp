using BackendController.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendController.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly SelectorPersistencia _selector;
    private readonly ILogger<ConfiguracionController> _logger;

    public ConfiguracionController(SelectorPersistencia selector, ILogger<ConfiguracionController> logger)
    {
        _selector = selector;
        _logger = logger;
    }

    [HttpGet("persistencia")]
    public IActionResult ObtenerPersistencia()
    {
        return Ok(new { modo = _selector.ModoActual });
    }

    [HttpPut("persistencia/{modo}")]
    public IActionResult CambiarPersistencia(string modo)
    {
        if (!modo.Equals(SelectorPersistencia.ModoDataSet, StringComparison.OrdinalIgnoreCase) &&
            !modo.Equals(SelectorPersistencia.ModoEF, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "Modo no válido. Usa DataSet o EF." });

        _selector.Cambiar(modo);
        _logger.LogInformation("El usuario {Usuario} ha cambiado la persistencia a {Modo}",
            User.Identity?.Name, _selector.ModoActual);

        return Ok(new { modo = _selector.ModoActual });
    }
}