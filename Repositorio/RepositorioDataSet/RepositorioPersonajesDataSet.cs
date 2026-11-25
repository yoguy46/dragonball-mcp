using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using Modelo.Entidades;
using Repositorio.Interfaces;

namespace Repositorio.RepositorioDataSet;

public class RepositorioPersonajesDataSet : IRepositorioPersonajes
{
    private readonly DataSet _dataSet = new("DragonBall");
    private readonly DataTable _tabla = new("Personajes");
    private readonly object _candado = new();
    private int _siguienteId = 1;

    public RepositorioPersonajesDataSet()
    {
        var id = _tabla.Columns.Add("Id", typeof(int));
        _tabla.Columns.Add("Nombre", typeof(string));
        _tabla.Columns.Add("NivelDePoder", typeof(decimal));
        _tabla.Columns.Add("SagaOPelicula", typeof(string));
        _tabla.Columns.Add("Serie", typeof(string));
        _tabla.PrimaryKey = new[] { id };
        _dataSet.Tables.Add(_tabla);
    }

    private static Personaje Mapear(DataRow fila) => new()
    {
        Id = fila.Field<int>("Id"),
        Nombre = fila.Field<string>("Nombre") ?? string.Empty,
        NivelDePoder = fila.Field<decimal>("NivelDePoder"),
        SagaOPelicula = fila.Field<string>("SagaOPelicula") ?? string.Empty,
        Serie = fila.Field<string>("Serie") ?? string.Empty
    };

    private void Añadir(Personaje p)
    {
        p.Id = _siguienteId++;
        _tabla.Rows.Add(p.Id, p.Nombre, p.NivelDePoder, p.SagaOPelicula, p.Serie);
    }

    public Task<IEnumerable<Personaje>> ObtenerTodosAsync()
    {
        lock (_candado)
        {
            IEnumerable<Personaje> lista = _tabla.AsEnumerable().Select(Mapear).ToList();
            return Task.FromResult(lista);
        }
    }

    public Task<Personaje?> ObtenerPorIdAsync(int id)
    {
        lock (_candado)
        {
            var fila = _tabla.Rows.Find(id);
            return Task.FromResult(fila is null ? null : Mapear(fila));
        }
    }

    public Task<IEnumerable<Personaje>> BuscarAsync(string? nombre, string? saga, string? serie)
    {
        lock (_candado)
        {
            var consulta = _tabla.AsEnumerable().Select(Mapear);

            if (!string.IsNullOrWhiteSpace(nombre))
                consulta = consulta.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(saga))
                consulta = consulta.Where(p => p.SagaOPelicula.Contains(saga, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(serie))
                consulta = consulta.Where(p => p.Serie.Contains(serie, StringComparison.OrdinalIgnoreCase));

            IEnumerable<Personaje> lista = consulta.ToList();
            return Task.FromResult(lista);
        }
    }

    public Task<Personaje> CrearAsync(Personaje personaje)
    {
        lock (_candado)
        {
            Añadir(personaje);
            return Task.FromResult(personaje);
        }
    }

    public Task<bool> ActualizarAsync(Personaje personaje)
    {
        lock (_candado)
        {
            var fila = _tabla.Rows.Find(personaje.Id);
            if (fila is null) return Task.FromResult(false);

            fila["Nombre"] = personaje.Nombre;
            fila["NivelDePoder"] = personaje.NivelDePoder;
            fila["SagaOPelicula"] = personaje.SagaOPelicula;
            fila["Serie"] = personaje.Serie;
            return Task.FromResult(true);
        }
    }

    public Task<bool> EliminarAsync(int id)
    {
        lock (_candado)
        {
            var fila = _tabla.Rows.Find(id);
            if (fila is null) return Task.FromResult(false);

            _tabla.Rows.Remove(fila);
            return Task.FromResult(true);
        }
    }

    public Task<int> AgregarVariosAsync(IEnumerable<Personaje> personajes)
    {
        lock (_candado)
        {
            int total = 0;
            foreach (var p in personajes)
            {
                Añadir(p);
                total++;
            }
            return Task.FromResult(total);
        }
    }

    public Task VaciarAsync()
    {
        lock (_candado)
        {
            _tabla.Clear();
            _siguienteId = 1;
            return Task.CompletedTask;
        }
    }
}