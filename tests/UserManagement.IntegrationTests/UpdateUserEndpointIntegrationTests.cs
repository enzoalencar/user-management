using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Users.UpdateUser;
using UserManagement.Domain.Users;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class UpdateUserEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public UpdateUserEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task PutUsers_WhenUpdatingOwnAccount_ShouldPersistChangesWithoutChangingPasswordOrStatus()
    {
        var seeded = await UsersEndpointTestHost.SeedUserAsync(_fixture, "update");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, seeded);
        seeded.IsActive = false;
        await _fixture.Repository.UpdateAsync(seeded);

        var requestBody = new UpdateUserRequest
        {
            FirstName = " Updated ",
            LastName = "Name",
            DateOfBirth = new DateTime(1993, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Email = $"updated.{Guid.NewGuid():N}@test.com",
            DocumentNumber = "UPDATED-DOC",
            PhoneNumber = ["+5511888888888"]
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{seeded.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UpdateUserResult>();
        Assert.NotNull(body);
        Assert.Equal(seeded.Id, body.Id);
        Assert.Equal("Updated", body.FirstName);
        Assert.Equal(requestBody.Email, body.Email);

        var persisted = await _fixture.Repository.FindOneAsync(seeded.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Updated", persisted.FirstName);
        Assert.Equal(requestBody.Email, persisted.Email);
        Assert.Equal(seeded.Password, persisted.Password);
        Assert.False(persisted.IsActive);
    }

    [Fact]
    public async Task PutUsers_WhenCommonUserUpdatesAnotherAccount_ShouldReturnForbidden()
    {
        var target = await UsersEndpointTestHost.SeedUserAsync(_fixture, "update-forbidden-target");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(
            _httpClient,
            _fixture,
            "update-forbidden-auth");
        var requestBody = new UpdateUserRequest
        {
            FirstName = "Forbidden",
            LastName = "Update",
            DateOfBirth = target.DateOfBirth,
            Email = target.Email,
            DocumentNumber = target.DocumentNumber,
            PhoneNumber = target.PhoneNumber
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{target.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var persisted = await _fixture.Repository.FindOneAsync(target.Id);
        Assert.NotNull(persisted);
        Assert.Equal(target.FirstName, persisted.FirstName);
    }

    [Fact]
    public async Task PutUsers_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(
            _httpClient,
            _fixture,
            "update-auth-not-found",
            UserRole.Administrator);
        var requestBody = new UpdateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = $"notfound.{Guid.NewGuid():N}@test.com",
            DocumentNumber = "DOC-NOT-FOUND",
            PhoneNumber = ["+5511777777777"]
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutUsers_WhenAccessTokenIsMissing_ShouldReturnUnauthorized()
    {
        var requestBody = new UpdateUserRequest
        {
            FirstName = "NoAuth",
            LastName = "User",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = $"noauth.{Guid.NewGuid():N}@test.com",
            DocumentNumber = "DOC-NO-AUTH",
            PhoneNumber = ["+5511666666666"]
        };

        var response = await _httpClient.PutAsJsonAsync($"/users/{Guid.NewGuid()}", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
