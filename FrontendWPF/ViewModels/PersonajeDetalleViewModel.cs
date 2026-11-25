using System.Windows.Input;
using FrontendWPF.Infraestructura;
using Modelo.Entidades;

namespace FrontendWPF.ViewModels;

// VISTA DETAIL: inserta un personaje nuevo o edita uno existente
public class PersonajeDetalleViewModel : ViewModelBase
{
    private readonly int _id;
    private string _nombre = "";
    private decimal _poder;
    private string _saga = "";
    private string _serie = "";
    private string _mensaje = "";

    public string Titulo { get; }

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    public decimal Poder
    {
        get => _poder;
        set => SetProperty(ref _poder, value);
    }

    public string Saga
    {
        get => _saga;
        set => SetProperty(ref _saga, value);
    }

    public string Serie
    {
        get => _serie;
        set => SetProperty(ref _serie, value);
    }

    public string Mensaje
    {
        get => _mensaje;
        set => SetProperty(ref _mensaje, value);
    }

    // Personaje tal y como ha quedado guardado en la API
    public Personaje? Resultado { get; private set; }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event Action<bool>? Cerrar;

    public PersonajeDetalleViewModel(Personaje? original)
    {
        if (original is null)
        {
            Titulo = "Nuevo personaje";
        }
        else
        {
            Titulo = "Editar personaje (Id " + original.Id + ")";
            _id = original.Id;
            Nombre = original.Nombre;
            Poder = original.NivelDePoder;
            Saga = original.SagaOPelicula;
            Serie = original.Serie;
        }

        GuardarCommand = new AsyncRelayCommand(_ => GuardarAsync());
        CancelarCommand = new RelayCommand(_ => Cerrar?.Invoke(false));
    }

    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            Mensaje = "El nombre es obligatorio.";
            return;
        }

        var personaje = new Personaje
        {
            Id = _id,
            Nombre = Nombre.Trim(),
            NivelDePoder = Poder,
            SagaOPelicula = Saga.Trim(),
            Serie = Serie.Trim()
        };

        if (_id == 0)
        {
            Resultado = await Sesion.Api.CrearAsync(personaje);
        }
        else if (await Sesion.Api.ActualizarAsync(personaje))
        {
            Resultado = personaje;
        }
        else
        {
            Mensaje = "El personaje ya no existe.";
            return;
        }

        if (Resultado is not null)
            Cerrar?.Invoke(true);
    }
}
