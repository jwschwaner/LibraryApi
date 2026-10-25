namespace LibraryApi.Domain.Common;

public sealed class DomainException(string code, string? message = null) : Exception(message ?? code)
{
    public string Code { get; } = code;
}
