using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Users.DeleteUser;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class DeleteUserEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public DeleteUserEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task DeleteUsers_WhenUserExistsAndAccessTokenIsValid_ShouldReturnOkAndDeleteUser()
    {
        var seeded = await UsersEndpointTestHost.SeedUserAsync(_fixture, "delete");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, _fixture, "delete-auth");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{seeded.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DeleteUserResult>();
        Assert.NotNull(body);
        Assert.True(body.Deleted);

        var persisted = await _fixture.Repository.FindOneAsync(seeded.Id);
        Assert.Null(persisted);
    }

    [Fact]
    public async Task DeleteUsers_WhenUserDoesNotExist_ShouldReturnOkWithDeletedFalse()
    {
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, _fixture, "delete-auth-not-found");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DeleteUserResult>();
        Assert.NotNull(body);
        Assert.False(body.Deleted);
    }

    [Fact]
    public async Task DeleteUsers_WhenAccessTokenIsMissing_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.DeleteAsync($"/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
