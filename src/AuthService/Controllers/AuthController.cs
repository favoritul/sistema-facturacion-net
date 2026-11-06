using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("invitaciones")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InvitacionResponseDto>> CrearInvitacion(InvitacionRequestDto dto)
    {
        var resultado = await _authService.CrearInvitacionAsync(dto);

        if (!resultado.EsExitoso)
        {
            return resultado.TipoDeError switch
            {
                TipoErrorAuth.EmailYaRegistrado => Conflict(resultado.Error),
                _                               => BadRequest(resultado.Error)
            };
        }

        return Created(string.Empty, resultado.Valor);
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioResponseDto>> Registrar(RegistroRequestDto dto)
    {
        var resultado = await _authService.RegistrarAsync(dto);

        if (!resultado.EsExitoso)
        {
            return resultado.TipoDeError switch
            {
                TipoErrorAuth.InvitacionInvalida  => BadRequest(resultado.Error),
                TipoErrorAuth.InvitacionExpirada  => BadRequest(resultado.Error),
                TipoErrorAuth.EmailYaRegistrado   => Conflict(resultado.Error),
                _                                 => BadRequest(resultado.Error)
            };
        }

        return Created(string.Empty, resultado.Valor);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (!resultado.EsExitoso)
        {
            return resultado.TipoDeError switch
            {
                TipoErrorAuth.CredencialesInvalidas => Unauthorized(resultado.Error),
                TipoErrorAuth.UsuarioInactivo       => Forbid(),
                _                                   => BadRequest(resultado.Error)
            };
        }

        return Ok(resultado.Valor);
    }
}