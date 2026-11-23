using MongoDB.Bson;

namespace LibraryApi.Application.Common;

/// <summary>
/// Converts between .NET values and the BSON the mappings store, for pipelines written as
/// <see cref="BsonDocument"/>s. Guids are standard UUIDs (binary subtype 4) and a
/// <see cref="DateOnly"/> is a BSON date at midnight UTC.
/// </summary>
public static class BsonValues
{
    public static BsonBinaryData ToBson(this Guid id) => new(id, GuidRepresentation.Standard);

    public static BsonArray ToBson(this IEnumerable<Guid> ids) => new(ids.Select(ToBson));

    public static BsonDateTime ToBson(this DateOnly day) => new(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    public static DateOnly ToDateOnly(this BsonValue value) => DateOnly.FromDateTime(value.ToUniversalTime());

    public static int? ToNullableInt32(this BsonValue value) => value.IsBsonNull ? null : value.ToInt32();
}
