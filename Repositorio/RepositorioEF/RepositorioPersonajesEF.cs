using Microsoft.EntityFrameworkCore;
using Modelo.Entidades;
using Repositorio.Interfaces;

namespace Repositorio.RepositorioEF;

public class RepositorioPersonajesEF : IRepositorioPersonajes
{
    private readonly ContextoDragonBall _contexto;

    public RepositorioPersonajesEF(ContextoDragonBall contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Personaje>> ObtenerTodosAsync()
    {
        return await _contexto.Personajes.AsNoTracking().ToListAsync();
    }

    public async Task<Personaje?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Personajes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Personaje>> BuscarAsync(string? nombre, string? saga, string? serie)
    {
        IQueryable<Personaje> consulta = _contexto.Personajes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre))
            consulta = consulta.Where(p => p.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(saga))
            consulta = consulta.Where(p => p.SagaOPelicula.Contains(saga));
        if (!string.IsNullOrWhiteSpace(serie))
            consulta = consulta.Where(p => p.Serie.Contains(serie));

        return await consulta.ToListAsync();
    }

 

























   public async Task<Personaje> CrearAsync(Personaje personaje)
    {
        personaje.Id = 0;
        _contexto.Personajes.Add(personaje);
        await _contexto.SaveChangesAsync();
        return personaje;
    }

    public async Task<bool> ActualizarAsync(Personaje personaje)
    {
        var existente = await _contexto.Personajes.FindAsync(personaje.Id);
        if (existente is null) return false;

        existente.Nombre = personaje.Nombre;
        existente.NivelDePoder = personaje.NivelDePoder;
        existente.SagaOPelicula = personaje.SagaOPelicula;
 















       existente.Serie = personaje.Serie;

        await _contexto.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var existente = await _contexto.Personajes.FindAsync(id);
        if (existente is null) return false;

 










       _contexto.Personajes.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }

    public async Task<int> AgregarVariosAsync(IEnumerable<Personaje> personajes)
    {
        var lista = personajes.ToList();
 







       foreach (var p in lista) p.Id = 0;

        _contexto.Personajes.AddRange(lista);
        await _contexto.SaveChangesAsync();
        return lista.Count;
    }

 






   public async Task VaciarAsync()
    {
        await _contexto.Personajes.ExecuteDeleteAsync();
    }
}
