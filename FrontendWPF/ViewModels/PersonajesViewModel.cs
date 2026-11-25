using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FrontendWPF.Infraestructura;
using Modelo.Entidades;

namespace FrontendWPF.ViewModels;

// VISTA MASTER: muestra el conjunto de registros y permite eliminar.
// Insertar y editar se hacen en la vista Detail (ventana aparte).
public class PersonajesViewModel : ViewModelBase
{
    private const int TamPagina = 100;

    // Todos los resultados de la última búsqueda; en pantalla solo se van mostrando por tandas
    private List<Personaje> _todos = new();

    private string _filtroNombre = "";
    private string _filtroSaga = "";
    private string _filtroSerie = "";
    private Personaje? _seleccionado;
    private string _estado = "";

    public ObservableCollection<Personaje> Personajes { get; } = new();

    // Lo asigna la vista: recibe el personaje a editar (o null para uno nuevo) y devuelve el resultado guardado
    public Func<Personaje?, Personaje?>? AbrirDetalle { get; set; }

    public string FiltroNombre
    {
        get => _filtroNombre;
        set => SetProperty(ref _filtroNombre, value);
    }

    public string FiltroSaga
    {
        get => _filtroSaga;
        set => SetProperty(ref _filtroSaga, value);
    }

    public string FiltroSerie
    {
        get => _filtroSerie;
        set => SetProperty(ref _filtroSerie, value);
    }

    public Personaje? Seleccionado
    {
        get => _seleccionado;
        set => SetProperty(ref _seleccionado, value);
    }

    public string Estado
    {
        get => _estado;
        set => SetProperty(ref _estado, value);
    }

    public string Resumen => "Mostrando " + Personajes.Count + " de " + _todos.Count;

    public ICommand BuscarCommand { get; }
    public ICommand CargarMasCommand { get; }
    public ICommand NuevoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand EliminarCommand { get; }

    public PersonajesViewModel()
    {
        BuscarCommand = new AsyncRelayCommand(_ => BuscarAsync());
        CargarMasCommand = new RelayCommand(_ => MostrarSiguienteTanda(), _ => Personajes.Count < _todos.Count);
        NuevoCommand = new RelayCommand(_ => AbrirFormulario(null));
        EditarCommand = new RelayCommand(_ => AbrirFormulario(Seleccionado), _ => Seleccionado is not null);
        EliminarCommand = new AsyncRelayCommand(_ => EliminarAsync(), _ => Seleccionado is not null);

        // Carga inicial sin bloquear la ventana
        BuscarCommand.Execute(null);
    }

    private async Task BuscarAsync()
    {
        Estado = "Cargando...";
        _todos = await Sesion.Api.ListarAsync(FiltroNombre, FiltroSaga, FiltroSerie);
        Personajes.Clear();
        MostrarSiguienteTanda();
        Estado = "";
    }

    private void MostrarSiguienteTanda()
    {
        foreach (var p in _todos.Skip(Personajes.Count).Take(TamPagina))
            Personajes.Add(p);
        OnPropertyChanged(nameof(Resumen));
    }

    private void AbrirFormulario(Personaje? original)
    {
        var guardado = AbrirDetalle?.Invoke(original);
        if (guardado is null) return;

        if (original is null)
        {
            _todos.Insert(0, guardado);
            Personajes.Insert(0, guardado);
            Estado = "Personaje creado.";
        }
        else
        {
            var indice = Personajes.ToList().FindIndex(x => x.Id == guardado.Id);
            if (indice >= 0) Personajes[indice] = guardado;
            var indiceTodos = _todos.FindIndex(x => x.Id == guardado.Id);
            if (indiceTodos >= 0) _todos[indiceTodos] = guardado;
            Estado = "Personaje actualizado.";
        }

        Seleccionado = guardado;
        OnPropertyChanged(nameof(Resumen));
    }

    private async Task EliminarAsync()
    {
        if (Seleccionado is null) return;

        var respuesta = MessageBox.Show(
            "¿Eliminar a " + Seleccionado.Nombre + "?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (respuesta != MessageBoxResult.Yes) return;

        var id = Seleccionado.Id;
        if (await Sesion.Api.EliminarAsync(id))
        {
            _todos.RemoveAll(x => x.Id == id);
            var enPantalla = Personajes.FirstOrDefault(x => x.Id == id);
            if (enPantalla is not null) Personajes.Remove(enPantalla);

            Seleccionado = null;
            Estado = "Personaje eliminado.";
            OnPropertyChanged(nameof(Resumen));
        }
        else
        {
            Estado = "El personaje ya no existe.";
        }
    }
}
