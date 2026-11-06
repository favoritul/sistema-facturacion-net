# Sistema de Facturación — .NET 10

Sistema de facturación para pymes con arquitectura de microservicios, construido con .NET 10, Entity Framework Core y YARP.

Existe una [versión equivalente en Java con Spring Boot](https://github.com/favoritul/sistema-facturacion-java): el mismo dominio implementado en ambos ecosistemas para comparar decisiones de diseño.

---

## Arquitectura

```
                    ┌──────────────┐
   Cliente  ───────▶│   Gateway    │  único puerto expuesto (8080)
                    │    (YARP)    │  valida JWT y enruta
                    └──────┬───────┘
                           │   red interna de Docker
              ┌────────────┴────────────┐
              ▼                         ▼
      ┌───────────────┐         ┌───────────────┐
      │  AuthService  │         │ BillingService│   sin puertos publicados
      │  emite  JWT   │         │  valida  JWT  │
      └───────┬───────┘         └───────┬───────┘
              ▼                         ▼
         ┌─────────┐               ┌─────────┐
         │ authdb  │               │billingdb│   una BD por servicio
         └─────────┘               └─────────┘
```

| Servicio | Responsabilidad | Base de datos |
|---|---|---|
| **Gateway** | Punto de entrada único, enrutamiento, política de autorización por ruta | — |
| **AuthService** | Usuarios, invitaciones, login, emisión de JWT | `authdb` |
| **BillingService** | Clientes, productos, facturación | `billingdb` |

## Stack

**Backend** · .NET 10, ASP.NET Core, C#
**Datos** · Entity Framework Core 9, Pomelo MySQL, migraciones versionadas
**Seguridad** · JWT con firma asimétrica RSA (RS256), BCrypt, autorización por rol y por propiedad del recurso
**Gateway** · YARP (Yet Another Reverse Proxy)
**Testing** · xUnit, Moq, EF Core InMemory, WebApplicationFactory — 23 tests
**Infraestructura** · Docker multi-stage, Docker Compose
**Documentación** · OpenAPI, Scalar

## Arrancar

Requiere Docker Desktop.

```bash
docker compose up --build -d
docker compose ps        # esperar a que los MySQL estén (healthy)
```

Todo el sistema queda accesible en `http://localhost:8080`.

```bash
# 1. Login con el administrador creado por seed
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sistemafacturacion.local","password":"CambiaEstaPassword123"}'

# 2. Usar el accessToken devuelto
curl http://localhost:8080/api/clientes \
  -H "Authorization: Bearer <token>"
```

> **Credenciales de desarrollo.** Las contraseñas de este repositorio (`devpassword`, `CambiaEstaPassword123`) son de uso local y están versionadas a propósito, para que el sistema arranque con un solo comando. En un entorno real irían en variables de entorno o en un gestor de secretos, y el `appsettings.Development.json` no se desplegaría.

## Decisiones de diseño

**Firma asimétrica RSA, no simétrica.** `AuthService` firma con la clave privada; los demás servicios solo tienen la pública. Un servicio comprometido no puede emitir tokens falsos. La clave privada está en `.gitignore`; solo la pública se versiona.

**Aislamiento de red real.** Únicamente el Gateway publica puertos. Los microservicios corren sin mapeo: no existe ruta hacia ellos desde fuera de la red de Docker.

**Defensa en profundidad.** El Gateway valida el token *y* cada microservicio lo valida de nuevo. Es trabajo duplicado deliberado: validar una firma RSA cuesta milisegundos, asumir que nadie puede saltarse el Gateway cuesta un incidente.

**Filtrar no es autorizar.** Las listas se filtran en la consulta SQL, y además cada recurso individual comprueba su propiedad. Sin lo segundo quedaría un IDOR: bastaría adivinar un id.

**404 en lugar de 403 al pedir un recurso ajeno por id.** Con 403 un atacante podría enumerar ids válidos probando números.

**El precio se congela al emitir la factura.** `LineaFactura` guarda su propio `PrecioUnitario` en lugar de leerlo del producto: una factura es un documento legal y debe reflejar lo que se cobró, no el precio actual.

**Invitaciones con token opaco, no JWT.** Un JWT es stateless y no se puede revocar; una invitación debe ser de un solo uso. Se guarda solo su hash SHA-256.

**Multi-stage build.** La imagen de producción no lleva compilador ni código fuente: 356 MB frente a cerca de un giga.

## Estructura

```
src/
├── AuthService/        Usuarios, invitaciones, emisión de JWT
├── BillingService/     Clientes, productos, facturas
└── Gateway/            YARP: enrutamiento y validación
tests/
└── BillingService.Tests/
    ├── Services/       Unitarios con EF Core InMemory
    ├── Controllers/    Con Moq sobre IFacturaService
    └── Integration/    WebApplicationFactory, peticiones HTTP reales
```

## Tests

```bash
dotnet test
```

23 tests en tres niveles: servicio (lógica de negocio y autorización), controlador (traducción a HTTP, con mocks) e integración (pipeline completo con autenticación simulada).
