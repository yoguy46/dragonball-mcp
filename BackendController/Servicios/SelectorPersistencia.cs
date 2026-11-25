using BackendController.Configuracion;
using Microsoft.Extensions.Options;
using Repositorio.Interfaces;
using Repositorio.RepositorioDataSet;
using Repositorio.RepositorioEF;

namespace BackendController.Servicios;

public class SelectorPersistencia
{
    public const string ModoDataSet = "DataSet";
    public const string ModoEF = "EF";

    public string ModoActual { get; private set; }

    public SelectorPersistencia(IOptionsMonitor<OpcionesPersistencia> opciones)
    {
        ModoActual = Normalizar(opciones.CurrentValue.Modo);
    }

    public void Cambiar(string modo) => ModoActual = Normalizar(modo);

    public IRepositorioPersonajes Obtener(IServiceProvider servicios, string? modo = null)
    {
        return Normalizar(modo ?? ModoActual) == ModoDataSet
            ? servicios.GetRequiredService<RepositorioPersonajesDataSet>()
            : servicios.GetRequiredService<RepositorioPersonajesEF>();
    }

    private static string Normalizar(string? modo) =>
        string.Equals(modo, ModoDataSet, StringComparison.OrdinalIgnoreCase) ? ModoDataSet : ModoEF;
}