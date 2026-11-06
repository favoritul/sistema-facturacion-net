using BillingService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace BillingService.Tests.Integration;

public class BillingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _nombreBaseDatos = Guid.NewGuid().ToString();

    public string Rol { get; set; } = "Admin";
    public int? ClienteId { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BillingDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BillingDbContext>>();

            services.AddDbContext<BillingDbContext>(options =>
                options.UseInMemoryDatabase(_nombreBaseDatos));

            // Sustituye JWT por un esquema de prueba que siempre autentica
            services.AddAuthentication(AuthHandlerDePrueba.Esquema)
                .AddScheme<OpcionesAuthDePrueba, AuthHandlerDePrueba>(
                    AuthHandlerDePrueba.Esquema,
                    opciones =>
                    {
                        opciones.Rol = Rol;
                        opciones.ClienteId = ClienteId;
                    });
        });
    }
}