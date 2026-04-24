using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Domain.Users;

namespace UserManagement.IntegrationTests;

internal static class UsersEndpointTestHost
{
    private const string JwtIssuer = "https://localhost:7015";
    private const string JwtAudience = "https://localhost:7015";
    private const string JwtSecretKey = "integration-tests-secret-key-32chars-min";
    private const string SeededPassword = "MyStrongPassword123!";

    public static WebApplicationFactory<Program> CreateFactory(MongoFixture fixture)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var testSettings = new Dictionary<string, string?>
                    {
                        ["MongoDb:ConnectionString"] = fixture.ConnectionString,
                        ["MongoDb:DatabaseName"] = fixture.DatabaseName,
                        ["MongoDb:UsersCollectionName"] = fixture.UsersCollectionName,
                        ["MongoDb:RefreshTokensCollectionName"] = fixture.RefreshTokensCollectionName,
                        ["Jwt:Issuer"] = JwtIssuer,
                        ["Jwt:Audience"] = JwtAudience,
                        ["Jwt:SecretKey"] = JwtSecretKey,
                        ["Jwt:AccessTokenExpirationInMinutes"] = "30",
                        ["Jwt:RefreshTokenExpirationInDays"] = "5",
                        ["Jwt:RefreshTokenCookieName"] = "um_refresh_token"
                    };

                    configBuilder.AddInMemoryCollection(testSettings);
                });
            });
    }

    public static HttpClient CreateHttpsClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    public static async Task<User> SeedUserAsync(MongoFixture fixture, string suffix = "user")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Integration",
            LastName = "User",
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = $"users.{suffix}.{Guid.NewGuid():N}@test.com",
            Password = BCrypt.Net.BCrypt.HashPassword(SeededPassword),
            DocumentNumber = $"DOC-{Guid.NewGuid():N}",
            PhoneNumber = ["+5511999999999"],
            IsActive = true
        };

        await fixture.Repository.CreateAsync(user);
        return user;
    }

    public static async Task<string> CreateAccessTokenByLoginAsync(
        HttpClient httpClient,
        MongoFixture fixture,
        string suffix = "auth")
    {
        var authUser = await SeedUserAsync(fixture, suffix);

        var loginRequest = new LoginRequest
        {
            Email = authUser.Email,
            Password = SeededPassword
        };

        var response = await httpClient.PostAsJsonAsync("/auth/login", loginRequest);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Login failed in test setup. Status: {(int)response.StatusCode}.");

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (body is null || string.IsNullOrWhiteSpace(body.Token))
            throw new InvalidOperationException("Access token was not returned by /auth/login.");

        return body.Token;
    }
}
