using System.Windows.Input;
using FrontendWPF.Infraestructura;

namespace FrontendWPF.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _usuario = "";
    private string _password = "";
    private string _mensaje = "";

    public string Usuario
    {
        get => _usuario;
        set => SetProperty(ref _usuario, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Mensaje
    {
        get => _mensaje;
        set => SetProperty(ref _mensaje, value);
    }

    public ICommand EntrarCommand { get; }

    public event Action? LoginCorrecto;

    public LoginViewModel()
    {
        EntrarCommand = new AsyncRelayCommand(async _ =>
        {
            Mensaje = "";
            try
            {
                await Sesion.Api.LoginAsync(Usuario, Password);
                Sesion.Usuario = Usuario;
                LoginCorrecto?.Invoke();
            }
            catch (Exception ex)
            {
                Mensaje = "No se pudo iniciar sesión: " + ex.Message;
            }
        });
    }
}
