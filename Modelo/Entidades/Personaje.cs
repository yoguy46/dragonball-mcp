using System;
using System.Collections.Generic;
using System.Text;

namespace Modelo.Entidades;

public class Personaje
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal NivelDePoder { get; set; }
    public string SagaOPelicula { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
}