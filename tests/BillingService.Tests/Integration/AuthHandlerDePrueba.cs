using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillingService.Tests.Integration;

public class OpcionesAuthDePrueba : AuthenticationSchemeOptions
{
    public string Rol { get; set; } = "Admin";
    public int? ClienteId { get; set; }
}

public class AuthHandlerDePrueba : AuthenticationHandler<OpcionesAuthDePrueba>
{
    public const string Esquema = "TestScheme";

    public AuthHandlerDePrueba(
        IOptionsMonitor<OpcionesAuthDePrueba> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new("role", Options.Rol)
        };

        if (Options.ClienteId.HasValue)
            claims.Add(new Claim("clienteId", Options.ClienteId.Value.ToString()));

        var identidad = new ClaimsIdentity(claims, Esquema, ClaimTypes.NameIdentifier, "role");
        var principal = new ClaimsPrincipal(identidad);
        var ticket = new AuthenticationTicket(principal, Esquema);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}