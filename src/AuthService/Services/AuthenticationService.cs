using AuthService.Data;
using AuthService.Dtos;
using AuthService.Entities;
using AuthService.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AuthService.Security;

namespace AuthService.Services;

public class AuthenticationService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthenticationService(
        AuthDbContext context,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<ResultadoAuth<InvitacionResponseDto>> CrearInvitacionAsync(InvitacionRequestDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        var yaRegistrado = await _context.Usuarios
            .AnyAsync(u => u.Email == emailNormalizado);

        if (yaRegistrado)
            return ResultadoAuth<InvitacionResponseDto>.Fallo(
                "Ya existe un usuario con ese email",
                TipoErrorAuth.EmailYaRegistrado);

        var token = TokenAleatorio.Generar();
        var expiracion = DateTime.UtcNow.AddDays(dto.DiasValidez);

        var invitacion = new Invitacion
        {
            Email = emailNormalizado,
            ClienteId = dto.ClienteId!.Value,
            TokenHash = TokenAleatorio.Hashear(token),
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = expiracion
        };

        _context.Invitaciones.Add(invitacion);
        await _context.SaveChangesAsync();

        return ResultadoAuth<InvitacionResponseDto>.Exito(
            new InvitacionResponseDto(
                Token: token,
                Email: emailNormalizado,
                ClienteId: invitacion.ClienteId,
                FechaExpiracion: expiracion));
    }

    public async Task<ResultadoAuth<UsuarioResponseDto>> RegistrarAsync(RegistroRequestDto dto)
    {
        var tokenHash = TokenAleatorio.Hashear(dto.TokenInvitacion);

        var invitacion = await _context.Invitaciones
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash);

        if (invitacion is null || invitacion.FechaUso.HasValue)
            return ResultadoAuth<UsuarioResponseDto>.Fallo(
                "La invitación no es válida o ya se ha utilizado",
                TipoErrorAuth.InvitacionInvalida);

        if (invitacion.FechaExpiracion < DateTime.UtcNow)
            return ResultadoAuth<UsuarioResponseDto>.Fallo(
                "La invitación ha caducado",
                TipoErrorAuth.InvitacionExpirada);

        var yaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == invitacion.Email);

        if (yaExiste)
            return ResultadoAuth<UsuarioResponseDto>.Fallo(
                "Ya existe un usuario con ese email",
                TipoErrorAuth.EmailYaRegistrado);

        var usuario = new Usuario
        {
            Email = invitacion.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = "Cliente",
            ClienteId = invitacion.ClienteId,
            FechaAlta = DateTime.UtcNow,
            Activo = true
        };

        invitacion.FechaUso = DateTime.UtcNow;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return ResultadoAuth<UsuarioResponseDto>.Exito(
            new UsuarioResponseDto(usuario.Id, usuario.Email, usuario.Rol, usuario.ClienteId));
    }

    public async Task<ResultadoAuth<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

        // Mismo mensaje tanto si el usuario no existe como si la contraseña falla
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return ResultadoAuth<LoginResponseDto>.Fallo(
                "Email o contraseña incorrectos",
                TipoErrorAuth.CredencialesInvalidas);

        if (!usuario.Activo)
            return ResultadoAuth<LoginResponseDto>.Fallo(
                "La cuenta está desactivada",
                TipoErrorAuth.UsuarioInactivo);

        var token = _tokenService.GenerarToken(usuario);

        return ResultadoAuth<LoginResponseDto>.Exito(
            new LoginResponseDto(
                AccessToken: token,
                TokenType: "Bearer",
                ExpiraEnSegundos: _jwtOptions.AccessTokenMinutes * 60));
    }
}