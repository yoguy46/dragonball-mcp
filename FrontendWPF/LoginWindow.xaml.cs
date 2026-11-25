using System.Windows;
using FrontendWPF.ViewModels;

namespace FrontendWPF;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        // El PasswordBox no admite binding, así que pasamos el valor al ViewModel a mano
        CajaPassword.PasswordChanged += (_, _) => _vm.Password = CajaPassword.Password;

        _vm.LoginCorrecto += () =>
        {
            new MainWindow().Show();
            Close();
        };
    }
}
