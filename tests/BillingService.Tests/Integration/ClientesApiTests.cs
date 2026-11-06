using System.Net;
using System.Net.Http.Json;
using BillingService.Dtos;
using FluentAssertions;

namespace BillingService.Tests.Integration;

public class ClientesApiTests : IClassFixture<BillingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClientesApiTests(BillingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCliente_SinNif_DevuelveBadRequest()
    {
        // ── ARRANGE ── payload al que le falta el NIF
        var payload = new
        {
            nombre = "Empresa sin NIF SL",
            email = "contacto@ejemplo.es"
        };

        // ── ACT ──
        var respuesta = await _client.PostAsJsonAsync("/api/clientes", payload);

        // ── ASSERT ──
        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        cuerpo.Should().Contain("NIF");
    }

    [Fact]
    public async Task PostCliente_ConEmailInvalido_DevuelveBadRequest()
    {
        var payload = new
        {
            nombre = "Empresa Test SL",
            nif = "B99999991",
            email = "esto-no-es-un-email"
        };

        var respuesta = await _client.PostAsJsonAsync("/api/clientes", payload);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostCliente_ConDatosValidos_DevuelveCreatedConLocation()
    {
        var payload = new
        {
            nombre = "Construcciones Ejemplo SL",
            nif = "B99999992",
            email = "admin@ejemplo.es"
        };

        var respuesta = await _client.PostAsJsonAsync("/api/clientes", payload);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
        respuesta.Headers.Location.Should().NotBeNull();

        var creado = await respuesta.Content.ReadFromJsonAsync<ClienteResponseDto>();
        creado.Should().NotBeNull();
        creado!.Id.Should().BeGreaterThan(0);
        creado.Nif.Should().Be("B99999992");
    }

    [Fact]
    public async Task GetCliente_ConIdInexistente_DevuelveNotFound()
    {
        var respuesta = await _client.GetAsync("/api/clientes/99999");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCliente_ConIdNoNumerico_DevuelveNotFound()
    {
        var respuesta = await _client.GetAsync("/api/clientes/abc");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}