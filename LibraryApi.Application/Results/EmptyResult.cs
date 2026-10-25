namespace LibraryApi.Application.Results;

public sealed class EmptyResult
{
    public static readonly EmptyResult Value = new();

    private EmptyResult()
    {
    }
}
