namespace AuthService.Services;

public class ResultadoAuth<T>
{
    public bool EsExitoso { get; private init; }
    public T? Valor { get; private init; }
    public string? Error { get; private init; }
    public TipoErrorAuth? TipoDeError { get; private init; }

    public static ResultadoAuth<T> Exito(T valor) =>
        new() { EsExitoso = true, Valor = valor };

    public static ResultadoAuth<T> Fallo(string error, TipoErrorAuth tipo) =>
        new() { EsExitoso = false, Error = error, TipoDeError = tipo };
}

public enum TipoErrorAuth
{
    CredencialesInvalidas,
    EmailYaRegistrado,
    UsuarioInactivo,
    InvitacionInvalida,
    InvitacionExpirada
}