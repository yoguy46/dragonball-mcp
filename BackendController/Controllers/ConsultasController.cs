using BackendController.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelo.Dtos;

namespace BackendController.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultasController : ControllerBase
{
    private readonly ServicioConsultas _servicio;

    public ConsultasController(ServicioConsultas servicio)
    {
        _servicio = servicio;
    }

    [HttpPost]
    public async Task<ActionResult<RespuestaConsulta>> Consultar([FromBody] PeticionConsulta peticion, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(peticion.Texto))
            return BadRequest("La pregunta no puede estar vacía");

        return Ok(await _servicio.ConsultarAsync(peticion.Texto, ct));
    }
}