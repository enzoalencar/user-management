using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using UserManagement.Api.Features.Users.CreateUser;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class CreateUserEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public CreateUserEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var testSettings = new Dictionary<string, string?>
                    {
                        ["MongoDb:ConnectionString"] = _fixture.ConnectionString,
                        ["MongoDb:DatabaseName"] = _fixture.DatabaseName,
                        ["MongoDb:UsersCollectionName"] = _fixture.UsersCollectionName
                    };

                    configBuilder.AddInMemoryCollection(testSettings);
                });
            });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task PostUsers_WhenPayloadIsValid_ShouldReturnCreatedAndPersistUser()
    {
        var request = new CreateUserRequest
        {
            FirstName = "  Enzo  ",
            LastName = "Alencar",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "enzo.integration@test.com",
            Password = "MyStrongPassword123!",
            DocumentNumber = "12345678900",
            PhoneNumber = ["+5511999999999"]
        };

        var response = await _httpClient.PostAsJsonAsync("/users", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateUserResult>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("Enzo", body.FirstName);
        Assert.Equal(request.LastName, body.LastName);
        Assert.Equal(request.Email, body.Email);
        Assert.Equal(request.DocumentNumber, body.DocumentNumber);
        Assert.True(body.IsActive);
        Assert.Equal($"/users/{body.Id}", response.Headers.Location?.OriginalString);

        var persisted = await _fixture.Repository.FindOneAsync(body.Id);
        Assert.NotNull(persisted);
        Assert.Equal(body.Id, persisted.Id);
        Assert.Equal("Enzo", persisted.FirstName);
        Assert.Equal(request.Email, persisted.Email);
        Assert.True(persisted.IsActive);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
