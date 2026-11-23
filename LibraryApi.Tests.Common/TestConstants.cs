namespace LibraryApi.Tests.Common;

public static class TestConstants
{
    /// <summary>
    /// The current time in every test: builders stamp entities with it and the integration tests
    /// freeze the API's <see cref="TimeProvider"/> on it, so due dates and "overdue" are stable.
    /// </summary>
    public static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    public static DateOnly Today => DateOnly.FromDateTime(NowUtc);

    /// <summary>An id no seeded document has, for not-found cases.</summary>
    public static readonly Guid UnknownId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
