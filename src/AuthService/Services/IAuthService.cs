using AuthService.Dtos;

namespace AuthService.Services;

public interface IAuthService
{
    Task<ResultadoAuth<InvitacionResponseDto>> CrearInvitacionAsync(InvitacionRequestDto dto);
    Task<ResultadoAuth<UsuarioResponseDto>> RegistrarAsync(RegistroRequestDto dto);
    Task<ResultadoAuth<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
}