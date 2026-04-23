using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Infrastructure.DependencyInjection;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Auth.Refresh;
using UserManagement.Api.Features.Users.CreateUser;
using UserManagement.Api.Features.Users.DeleteUser;
using UserManagement.Api.Features.Users.FindAllUsers;
using UserManagement.Api.Features.Users.FindOneUser;
using UserManagement.Api.Features.Users.UpdateUser;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException($"'{JwtSettings.SectionName}' not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException($"'{JwtSettings.SectionName}:SecretKey' must have at least 32 characters.");

if (jwtSettings.AccessTokenExpirationInMinutes <= 0)
    throw new InvalidOperationException(
        $"'{JwtSettings.SectionName}:AccessTokenExpirationInMinutes' must be greater than zero.");

if (jwtSettings.RefreshTokenExpirationInDays <= 0)
    throw new InvalidOperationException(
        $"'{JwtSettings.SectionName}:RefreshTokenExpirationInMinutes' must be greater than zero.");

if (string.IsNullOrWhiteSpace(jwtSettings.RefreshTokenCookieName))
    throw new InvalidOperationException($"'{JwtSettings.SectionName}:RefreshTokenCookieName' must be configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(JwtRegisteredClaimNames.Sub)
            .RequireClaim(JwtRegisteredClaimNames.Email))
    .AddPolicy(AuthPolicies.ActiveUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(AuthClaimTypes.IsActive, "true"))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(JwtRegisteredClaimNames.Sub)
        .RequireClaim(JwtRegisteredClaimNames.Email)
        .Build());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RefreshHandler>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseMiddleware<UserManagement.Api.Utils.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapLogin();
app.MapRefresh();
app.MapCreateUser();
app.MapDeleteUser();
app.MapUpdateUser();
app.MapFindOneUser();
app.MapFindAllUser();

app.Run();
