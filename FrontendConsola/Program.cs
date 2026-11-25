using System.Globalization;
using FrontendConsola;
using Modelo.Entidades;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var api = new ClienteApi("http://localhost:5000");

Console.WriteLine("=== Dragon Ball - Cliente de consola ===");

try
{
    var usuario = Pedir("Usuario");
    var password = Pedir("Contraseña");
    await api.LoginAsync(usuario, password);
    Console.WriteLine("Sesión iniciada.");
}
catch (Exception ex)
{
    Console.WriteLine("No se pudo iniciar sesión: " + ex.Message);
    return;
}

var salir = false;
while (!salir)
{
    Console.WriteLine();
    Console.WriteLine("1. Importar CSV a MySQL (EF)");
    Console.WriteLine("2. Importar CSV a memoria (DataSet)");
    Console.WriteLine("3. Cambiar persistencia activa");
    Console.WriteLine("4. Listar / buscar personajes");
    Console.WriteLine("5. Añadir personaje");
    Console.WriteLine("6. Modificar personaje");
    Console.WriteLine("7. Eliminar personaje");
    Console.WriteLine("8. Preguntar en lenguaje natural (MCP)");
    Console.WriteLine("0. Salir");
    var opcion = Pedir("Opción");

    try
    {
        switch (opcion)
        {
            case "1":
                await Importar("EF");
                break;
            case "2":
                await Importar("DataSet");
                break;
            case "3":
                Console.WriteLine(await api.CambiarPersistenciaAsync(Pedir("Modo (EF o DataSet)")));
                break;
            case "4":
                await Listar();
                break;
            case "5":
                await Anadir();
                break;
            case "6":
                await Modificar();
                break;
            case "7":
                await Eliminar();
                break;
            case "8":
                await Preguntar();
                break;
            case "0":
                salir = true;
                break;
            default:
                Console.WriteLine("Opción no válida.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}

async Task Importar(string destino)
{
    Console.WriteLine("Importando a " + destino + "...");
    var r = await api.ImportarAsync(destino);
    Console.WriteLine("Importados: " + r?.Importados + "  Omitidos: " + r?.Omitidos);
}

async Task Listar()
{
    var nombre = Pedir("Nombre (vacío para no filtrar)");
    var saga = Pedir("Saga (vacío para no filtrar)");
    var serie = Pedir("Serie (vacío para no filtrar)");
    var lista = await api.ListarAsync(nombre, saga, serie);
    foreach (var p in lista.Take(20)) Mostrar(p);
    Console.WriteLine("Total: " + lista.Count + " (se muestran los 20 primeros)");
}

async Task Anadir()
{
    var nuevo = new Personaje
    {
        Nombre = Pedir("Nombre"),
        NivelDePoder = PedirDecimal("Nivel de poder"),
        SagaOPelicula = Pedir("Saga o película"),
        Serie = Pedir("Serie")
    };
    var creado = await api.CrearAsync(nuevo);
    Console.WriteLine("Creado:");
    if (creado is not null) Mostrar(creado);
}

async Task Modificar()
{
    var id = PedirEntero("Id del personaje");
    var p = await api.ObtenerAsync(id);
    if (p is null)
    {
        Console.WriteLine("No existe.");
        return;
    }

    Console.WriteLine("Deja vacío lo que no quieras cambiar.");
    var nombre = Pedir("Nombre [" + p.Nombre + "]");
    if (nombre != "") p.Nombre = nombre;

    var poder = Pedir("Nivel de poder [" + p.NivelDePoder + "]");
    if (poder != "" && decimal.TryParse(poder.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var nuevoPoder))
        p.NivelDePoder = nuevoPoder;

    var saga = Pedir("Saga o película [" + p.SagaOPelicula + "]");
    if (saga != "") p.SagaOPelicula = saga;

    var serie = Pedir("Serie [" + p.Serie + "]");
    if (serie != "") p.Serie = serie;

    Console.WriteLine(await api.ActualizarAsync(p) ? "Actualizado." : "No existe.");
}

async Task Eliminar()
{
    var id = PedirEntero("Id del personaje");
    var confirmar = Pedir("¿Seguro? (s/n)");
    if (confirmar.ToLower() == "s")
        Console.WriteLine(await api.EliminarAsync(id) ? "Eliminado." : "No existe.");
}

async Task Preguntar()
{
    var pregunta = Pedir("Pregunta");
    Console.WriteLine("Consultando...");
    var r = await api.ConsultarAsync(pregunta);
    if (r is null) return;

    var argumentos = string.Join(", ", r.Argumentos.Select(a => a.Key + "=" + a.Value));
    Console.WriteLine("Enrutador:   " + r.Enrutador);
    Console.WriteLine("Herramienta: " + r.Herramienta);
    Console.WriteLine("Argumentos:  " + argumentos);
    Console.WriteLine("Resultado:   " + r.Resultado);
}

string Pedir(string texto)
{
    Console.Write(texto + ": ");
    return (Console.ReadLine() ?? "").Trim();
}

int PedirEntero(string texto)
{
    while (true)
    {
        if (int.TryParse(Pedir(texto), out var n)) return n;
        Console.WriteLine("Escribe un número entero.");
    }
}

decimal PedirDecimal(string texto)
{
    while (true)
    {
        var t = Pedir(texto).Replace(',', '.');
        if (decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var n)) return n;
        Console.WriteLine("Escribe un número.");
    }
}

void Mostrar(Personaje p)
{
    var nombre = p.Nombre.Length > 35 ? p.Nombre.Substring(0, 35) : p.Nombre;
    Console.WriteLine(p.Id.ToString().PadLeft(5) + "  " + nombre.PadRight(35) + "  " + p.NivelDePoder.ToString("N0").PadLeft(22) + "  " + p.SagaOPelicula.PadRight(25) + "  " + p.Serie);
}
