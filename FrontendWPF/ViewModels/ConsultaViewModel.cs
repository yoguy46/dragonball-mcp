using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontendWPF.Infraestructura;

namespace FrontendWPF.ViewModels;

public record MensajeChat(string Autor, string Texto);

public class ConsultaViewModel : ViewModelBase
{
    private string _pregunta = "";

    public ObservableCollection<MensajeChat> Mensajes { get; } = new();

    public string Pregunta
    {
        get => _pregunta;
        set => SetProperty(ref _pregunta, value);
    }

    public ICommand EnviarCommand { get; }

    public ConsultaViewModel()
    {
        Mensajes.Add(new MensajeChat("Sistema",
            "Pregúntame por los personajes. Por ejemplo: cuántos personajes hay en la serie Dragon Ball Z, " +
            "busca Vegeta, o quién es el más poderoso."));

        EnviarCommand = new AsyncRelayCommand(_ => EnviarAsync(), _ => !string.IsNullOrWhiteSpace(Pregunta));
    }

    private async Task EnviarAsync()
    {
        var texto = Pregunta.Trim();
        Pregunta = "";
        Mensajes.Add(new MensajeChat("Tú", texto));
        Mensajes.Add(new MensajeChat("Sistema", "Consultando..."));
        var indice = Mensajes.Count - 1;

        try
        {
            var r = await Sesion.Api.ConsultarAsync(texto);
            if (r is null)
            {
                Mensajes[indice] = new MensajeChat("Sistema", "Sin respuesta de la API.");
                return;
            }

            var argumentos = string.Join(", ", r.Argumentos.Select(a => a.Key + "=" + a.Value));
            var detalle = "Enrutador: " + r.Enrutador;
            if (!string.IsNullOrEmpty(r.Herramienta))
                detalle += " | Herramienta: " + r.Herramienta + "(" + argumentos + ")";

            Mensajes[indice] = new MensajeChat("Sistema", detalle + "\n" + r.Resultado);
        }
        catch (Exception ex)
        {
            Mensajes[indice] = new MensajeChat("Sistema", "Error: " + ex.Message);
        }
    }
}
