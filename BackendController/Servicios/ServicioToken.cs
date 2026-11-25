using System.Security.Claims;
using System.Text;
using BackendController.Configuracion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Modelo.Dtos;

namespace BackendController.Servicios;

public class ServicioToken
{
    private readonly IOptionsMonitor<OpcionesJwt> _opciones;

    public ServicioToken(IOptionsMonitor<OpcionesJwt> opciones)
    {
        _opciones = opciones;
    }

    public RespuestaLogin Generar(string usuario)
    {
        var jwt = _opciones.CurrentValue;
        var expira = DateTime.UtcNow.AddMinutes(jwt.MinutosExpiracion);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, usuario)
            }),
            Expires = expira,
            Issuer = jwt.Emisor,
            Audience = jwt.Audiencia,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
                SecurityAlgorithms.HmacSha256)
        };

 
























       var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new RespuestaLogin { Token = token, Expira = expira };
    }
}
