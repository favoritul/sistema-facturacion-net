using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public static class SeedAdmin
{
    public static async Task EjecutarAsync(IServiceProvider servicios, IConfiguration config)
    {
        var email = config["AdminSeed:Email"];
        var password = config["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        using var scope = servicios.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var yaHayAdmin = await context.Usuarios.AnyAsync(u => u.Rol == "Admin");

        if (yaHayAdmin)
            return;

        var admin = new Usuario
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = "Admin",
            ClienteId = null,
            FechaAlta = DateTime.UtcNow,
            Activo = true
        };

        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();
    }
}