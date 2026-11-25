using System.Windows;
using FrontendWPF.ViewModels;
using Modelo.Entidades;

namespace FrontendWPF.Views;

public partial class PersonajeDetalleWindow : Window
{
    private readonly PersonajeDetalleViewModel _vm;

    public Personaje? Resultado => _vm.Resultado;

    public PersonajeDetalleWindow(Personaje? original)
    {
        InitializeComponent();

        _vm = new PersonajeDetalleViewModel(original);
        DataContext = _vm;
        _vm.Cerrar += aceptado => DialogResult = aceptado;
    }
}
