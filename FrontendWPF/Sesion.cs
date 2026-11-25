using System.Windows;
using FrontendWPF.Infraestructura;

namespace FrontendWPF;

// Datos compartidos por toda la aplicación: el cliente de la API (ya autenticado) y el usuario
public static class Sesion
{
    public static ClienteApi Api { get; } = new("http://localhost:5000");

    public static string Usuario { get; set; } = "";

    static Sesion()
    {
        // Cualquier error en un comando asíncrono se muestra en una ventana de aviso
        AsyncRelayCommand.ManejadorErrores = ex =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
