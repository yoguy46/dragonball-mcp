using System.Windows;
using System.Windows.Controls;
using FrontendWPF.ViewModels;
using Modelo.Entidades;

namespace FrontendWPF.Views;

public partial class PersonajesView : UserControl
{
    public PersonajesView()
    {
        InitializeComponent();

        // Cuando la vista recibe su ViewModel, le damos la forma de abrir la ventana Detail
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PersonajesViewModel vm)
                vm.AbrirDetalle = AbrirDetalle;
        };
    }

    private Personaje? AbrirDetalle(Personaje? original)
    {
        var ventana = new PersonajeDetalleWindow(original)
        {
            Owner = Window.GetWindow(this)
        };
        return ventana.ShowDialog() == true ? ventana.Resultado : null;
    }
}
