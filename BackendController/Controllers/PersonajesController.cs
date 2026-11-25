using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelo.Entidades;
using Repositorio.Interfaces;

namespace BackendController.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PersonajesController : ControllerBase
{
    private readonly IRepositorioPersonajes _repositorio;
    private readonly ILogger<PersonajesController> _logger;

    public PersonajesController(IRepositorioPersonajes repositorio, ILogger<PersonajesController> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Personaje>>> Listar(
        [FromQuery] string? nombre,
        [FromQuery] string? saga,
        [FromQuery] string? serie)
    {
        var resultado = await _repositorio.BuscarAsync(nombre, saga, serie);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Personaje>> Obtener(int id)
    {
        var personaje = await _repositorio.ObtenerPorIdAsync(id);
        return personaje is null ? NotFound() : Ok(personaje);
    }

    [HttpPost]
    public async Task<ActionResult<Personaje>> Crear([FromBody] Personaje personaje)
    {
        var creado = await _repositorio.CrearAsync(personaje);
        _logger.LogInformation("El usuario {Usuario} ha creado el personaje {Id}", User.Identity?.Name, creado.Id);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Personaje personaje)
    {
        personaje.Id = id;
        var actualizado = await _repositorio.ActualizarAsync(personaje);
        if (!actualizado) return NotFound();

        _logger.LogInformation("El usuario {Usuario} ha actualizado el personaje {Id}", User.Identity?.Name, id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _repositorio.EliminarAsync(id);
        if (!eliminado) return NotFound();

        _logger.LogInformation("El usuario {Usuario} ha eliminado el personaje {Id}", User.Identity?.Name, id);
        return NoContent();
    }
}