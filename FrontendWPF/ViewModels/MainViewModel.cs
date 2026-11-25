using System.Windows.Input;
using FrontendWPF.Infraestructura;

namespace FrontendWPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private object _vistaActual = null!;

    // La ventana muestra aquí el ViewModel activo; un DataTemplate decide qué vista le corresponde
    public object VistaActual
    {
        get => _vistaActual;
        set => SetProperty(ref _vistaActual, value);
    }

    public string Usuario => "Usuario: " + Sesion.Usuario;

    public ICommand MostrarConfiguracionCommand { get; }
    public ICommand MostrarPersonajesCommand { get; }
    public ICommand MostrarConsultaCommand { get; }

    public MainViewModel()
    {
        MostrarConfiguracionCommand = new RelayCommand(_ => VistaActual = new ConfiguracionViewModel());
        MostrarPersonajesCommand = new RelayCommand(_ => VistaActual = new PersonajesViewModel());
        MostrarConsultaCommand = new RelayCommand(_ => VistaActual = new ConsultaViewModel());

        VistaActual = new ConfiguracionViewModel();
    }
}
