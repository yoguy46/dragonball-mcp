using System.Windows.Input;

namespace FrontendWPF.Infraestructura;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _ejecutar;
    private readonly Func<object?, bool>? _puedeEjecutar;

    public RelayCommand(Action<object?> ejecutar, Func<object?, bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parametro) => _puedeEjecutar?.Invoke(parametro) ?? true;

    public void Execute(object? parametro) => _ejecutar(parametro);
}

// Comando asíncrono: no deja pulsar el botón dos veces mientras la tarea sigue en marcha
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _ejecutar;
    private readonly Func<object?, bool>? _puedeEjecutar;
    private bool _enCurso;

    public static Action<Exception>? ManejadorErrores { get; set; }

    public AsyncRelayCommand(Func<object?, Task> ejecutar, Func<object?, bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parametro) => !_enCurso && (_puedeEjecutar?.Invoke(parametro) ?? true);

    public async void Execute(object? parametro)
    {
        _enCurso = true;
        CommandManager.InvalidateRequerySuggested();
        try
        {
            await _ejecutar(parametro);
        }
        catch (Exception ex)
        {
            ManejadorErrores?.Invoke(ex);
        }
        finally
        {
            _enCurso = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
