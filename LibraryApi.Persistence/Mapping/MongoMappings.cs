using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Common;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace LibraryApi.Persistence.Mapping;

/// <summary>
/// Maps the domain entities to BSON here, so the domain stays free of MongoDB attributes.
/// The driver keeps class maps in static state, so this must run once per process.
/// </summary>
internal static class MongoMappings
{
    private static readonly Lock Gate = new();
    private static bool _registered;

    public static void Register()
    {
        lock (Gate)
        {
            if (_registered)
                return;

            ConventionRegistry.Register(
                "LibraryApi",
                new ConventionPack { new CamelCaseElementNameConvention(), new IgnoreExtraElementsConvention(true) },
                _ => true);

            // Store Guids as standard UUID binary (subtype 4) rather than leaving it to the driver
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(e => e.Id);
            });
            BsonClassMap.RegisterClassMap<AuditedEntity>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Author>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Book>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Member>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Loan>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(l => l.IsReturned);
            });

            _registered = true;
        }
    }
}
