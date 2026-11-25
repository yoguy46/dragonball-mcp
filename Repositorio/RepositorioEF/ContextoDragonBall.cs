using Microsoft.EntityFrameworkCore;
using Modelo.Entidades;

namespace Repositorio.RepositorioEF;

public class ContextoDragonBall : DbContext
{
    public ContextoDragonBall(DbContextOptions<ContextoDragonBall> opciones) : base(opciones)
    {
    }

    public DbSet<Personaje> Personajes => Set<Personaje>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Personaje>(entidad =>
        {
            entidad.ToTable("personajes");
            entidad.HasKey(p => p.Id);
            entidad.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            entidad.Property(p => p.NivelDePoder).HasPrecision(30, 2);
            entidad.Property(p => p.SagaOPelicula).IsRequired().HasMaxLength(200);
            entidad.Property(p => p.Serie).IsRequired().HasMaxLength(100);
        });
    }
}
