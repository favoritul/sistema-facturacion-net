using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using AuthService.Configuration;
using AuthService.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opciones;
    private readonly RsaSecurityKey _claveFirma;

    public TokenService(IOptions<JwtOptions> opciones)
    {
        _opciones = opciones.Value;

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(_opciones.PrivateKeyPath));

        _claveFirma = new RsaSecurityKey(rsa);
    }

    public string GenerarToken(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", usuario.Rol)
        };

        if (usuario.ClienteId.HasValue)
            claims.Add(new Claim("clienteId", usuario.ClienteId.Value.ToString()));

        var credenciales = new SigningCredentials(
            _claveFirma, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahora,
            expires: ahora.AddMinutes(_opciones.AccessTokenMinutes),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}