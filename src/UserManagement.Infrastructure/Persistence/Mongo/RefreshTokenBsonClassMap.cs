using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using UserManagement.Domain.Auth;

namespace UserManagement.Infrastructure.Persistence.Mongo;

public static class RefreshTokenBsonClassMap
{
    private static int _initialized;

    public static void Register()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        if (BsonClassMap.IsClassMapRegistered(typeof(RefreshToken)))
            return;

        BsonClassMap.RegisterClassMap<RefreshToken>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapIdMember(refreshToken => refreshToken.Id)
                .SetElementName("_id")
                .SetIdGenerator(GuidGenerator.Instance)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

            classMap.MapMember(refreshToken => refreshToken.UserId)
                .SetElementName("userId")
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            classMap.MapMember(refreshToken => refreshToken.TokenHash).SetElementName("tokenHash");
            classMap.MapMember(refreshToken => refreshToken.CreatedAtUtc)
                .SetElementName("createdAtUtc")
                .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            classMap.MapMember(refreshToken => refreshToken.ExpiresAtUtc)
                .SetElementName("expiresAtUtc")
                .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            classMap.MapMember(refreshToken => refreshToken.RevokedAtUtc)
                .SetElementName("revokedAtUtc")
                .SetSerializer(new NullableSerializer<DateTime>(
                    new DateTimeSerializer(DateTimeKind.Utc)));
            classMap.MapMember(refreshToken => refreshToken.ReplacedByTokenHash)
                .SetElementName("replacedByTokenHash");
        });
    }
}
