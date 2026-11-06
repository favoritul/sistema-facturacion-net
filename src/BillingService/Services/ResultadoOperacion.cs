namespace BillingService.Services;

public class ResultadoOperacion<T>
{
    public bool EsExitoso { get; private init; }
    public T? Valor { get; private init; }
    public string? Error { get; private init; }
    public TipoError? TipoDeError { get; private init; }

    public static ResultadoOperacion<T> Exito(T valor) =>
        new() { EsExitoso = true, Valor = valor };

    public static ResultadoOperacion<T> Fallo(string error, TipoError tipo) =>
        new() { EsExitoso = false, Error = error, TipoDeError = tipo };
}

public enum TipoError
{
    NoEncontrado,
    ReglaDeNegocio,
    AccesoDenegado
}