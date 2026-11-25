using BackendController.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modelo.Dtos;

namespace BackendController.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ImportacionController : ControllerBase
{
    private readonly ImportadorCsv _importador;
    private readonly SelectorPersistencia _selector;
    private readonly ILogger<ImportacionController> _logger;

    public ImportacionController(
        ImportadorCsv importador,
        SelectorPersistencia selector,
        ILogger<ImportacionController> logger)
    {
        _importador = importador;
        _selector = selector;
        _logger = logger;
    }

    // destino: DataSet (memoria) o EF (MySQL)
    [HttpPost("{destino}")]
    public async Task<ActionResult<ResultadoImportacion>> Importar(string destino)
    {
        string modo;
        if (destino.Equals(SelectorPersistencia.ModoDataSet, StringComparison.OrdinalIgnoreCase))
            modo = SelectorPersistencia.ModoDataSet;
        else if (destino.Equals(SelectorPersistencia.ModoEF, StringComparison.OrdinalIgnoreCase))
            modo = SelectorPersistencia.ModoEF;
        else
            return BadRequest(new { mensaje = "Destino no válido. Usa DataSet o EF." });

        var repositorio = _selector.Obtener(HttpContext.RequestServices, modo);
        var (personajes, omitidos) = await _importador.LeerAsync();

        await repositorio.VaciarAsync();
        var total = await repositorio.AgregarVariosAsync(personajes);

        _logger.LogInformation("El usuario {Usuario} ha importado {Total} personajes en {Destino}",
            User.Identity?.Name, total, modo);

        return Ok(new ResultadoImportacion { Destino = modo, Importados = total, Omitidos = omitidos });
    }
}