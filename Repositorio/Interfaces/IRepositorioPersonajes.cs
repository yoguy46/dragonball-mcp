using System;
using System.Collections.Generic;
using System.Text;

using Modelo.Entidades;

namespace Repositorio.Interfaces;

public interface IRepositorioPersonajes
{
    Task<IEnumerable<Personaje>> ObtenerTodosAsync();
    Task<Personaje?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Personaje>> BuscarAsync(string? nombre, string? saga, string? serie);
    Task<Personaje> CrearAsync(Personaje personaje);
    Task<bool> ActualizarAsync(Personaje personaje);
    Task<bool> EliminarAsync(int id);
    Task<int> AgregarVariosAsync(IEnumerable<Personaje> personajes);
    Task VaciarAsync();
}