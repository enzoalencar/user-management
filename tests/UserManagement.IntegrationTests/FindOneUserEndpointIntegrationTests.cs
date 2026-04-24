using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.Api.Features.Users.FindOneUser;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class FindOneUserEndpointIntegrationTests : IClassFixture<MongoFixture>, IDisposable
{
    private readonly MongoFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public FindOneUserEndpointIntegrationTests(MongoFixture fixture)
    {
        _fixture = fixture;
        _factory = UsersEndpointTestHost.CreateFactory(_fixture);
        _httpClient = UsersEndpointTestHost.CreateHttpsClient(_factory);
    }

    [Fact]
    public async Task GetUserById_WhenUserExistsAndAccessTokenIsValid_ShouldReturnOk()
    {
        var seeded = await UsersEndpointTestHost.SeedUserAsync(_fixture, "find-one");
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, _fixture, "find-one-auth");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/users/{seeded.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<FindOneUserResult>();
        Assert.NotNull(body);
        Assert.Equal(seeded.Id, body.Id);
        Assert.Equal(seeded.Email, body.Email);
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var token = await UsersEndpointTestHost.CreateAccessTokenByLoginAsync(_httpClient, _fixture, "find-one-auth-not-found");
        var unknownId = Guid.NewGuid();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/users/{unknownId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WhenAccessTokenIsMissing_ShouldReturnUnauthorized()
    {
        var response = await _httpClient.GetAsync($"/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }
}
