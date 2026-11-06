namespace BillingService.Services;

public interface ICurrentUser
{
    int? ClienteId { get; }
    bool EsAdmin { get; }
    bool EstaAutenticado { get; }
}