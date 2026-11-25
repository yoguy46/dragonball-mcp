using System.Text.Json;
using System.Windows.Input;
using FrontendWPF.Infraestructura;

namespace FrontendWPF.ViewModels;

public class ConfiguracionViewModel : ViewModelBase
{
    private string _modoActual = "EF (valor de la configuración)";
    private string _estado = "";

    public string ModoActual
    {
        get => _modoActual;
        set => SetProperty(ref _modoActual, value);
    }

    public string Estado
    {
        get => _estado;
        set => SetProperty(ref _estado, value);
    }

    public ICommand ImportarEFCommand { get; }
    public ICommand ImportarDataSetCommand { get; }
    public ICommand UsarEFCommand { get; }
    public ICommand UsarDataSetCommand { get; }

    public ConfiguracionViewModel()
    {
        ImportarEFCommand = new AsyncRelayCommand(_ => ImportarAsync("EF"));
        ImportarDataSetCommand = new AsyncRelayCommand(_ => ImportarAsync("DataSet"));
        UsarEFCommand = new AsyncRelayCommand(_ => CambiarModoAsync("EF"));
        UsarDataSetCommand = new AsyncRelayCommand(_ => CambiarModoAsync("DataSet"));
    }

    private async Task ImportarAsync(string destino)
    {
        Estado = "Importando a " + destino + "...";
        var resultado = await Sesion.Api.ImportarAsync(destino);
        Estado = "Importación en " + destino + " terminada: "
                 + resultado?.Importados + " importados, " + resultado?.Omitidos + " omitidos.";
    }

    private async Task CambiarModoAsync(string modo)
    {
        var json = await Sesion.Api.CambiarPersistenciaAsync(modo);
        using var doc = JsonDocument.Parse(json);
        ModoActual = doc.RootElement.GetProperty("modo").GetString() ?? modo;
        Estado = "Ahora la API trabaja con " + ModoActual + ".";
    }
}
