using System.Windows;
using FrontendWPF.ViewModels;

namespace FrontendWPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
