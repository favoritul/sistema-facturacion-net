using System.Security.Claims;
using BillingService.Services;

namespace BillingService.Infrastructure;

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
    }

    public bool EstaAutenticado =>
        _principal?.Identity?.IsAuthenticated ?? false;

    public bool EsAdmin =>
        _principal?.IsInRole("Admin") ?? false;

    public int? ClienteId
    {
        get
        {
            var valor = _principal?.FindFirst("clienteId")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}