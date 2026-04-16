using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using UserManagement.Domain.Users;

namespace UserManagement.Infrastructure.Persistence.Mongo;

public static class UserBsonClassMap
{
    private static int _initialized;

    public static void Register()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        if (BsonClassMap.IsClassMapRegistered(typeof(User)))
            return;

        BsonClassMap.RegisterClassMap<User>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapIdMember(user => user.Id)
                .SetElementName("_id")
                .SetIdGenerator(GuidGenerator.Instance)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

            classMap.MapMember(user => user.FirstName).SetElementName("firstName");
            classMap.MapMember(user => user.LastName).SetElementName("lastName");
            classMap.MapMember(user => user.DateOfBirth)
                .SetElementName("dateOfBirth")
                .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));

            classMap.MapMember(user => user.Email).SetElementName("email");
            classMap.MapMember(user => user.DocumentNumber).SetElementName("documentNumber");
            classMap.MapMember(user => user.PhoneNumber).SetElementName("phoneNumbers");
            classMap.MapMember(user => user.IsActive).SetElementName("isActive");
        });
    }
}
