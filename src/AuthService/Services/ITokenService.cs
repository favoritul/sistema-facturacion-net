using AuthService.Entities;

namespace AuthService.Services;

public interface ITokenService
{
    string GenerarToken(Usuario usuario);
}