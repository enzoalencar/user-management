namespace UserManagement.Api.Features.Auth.Jwt;

public sealed record IssuedTokens(string AccessToken, string RefreshToken);
