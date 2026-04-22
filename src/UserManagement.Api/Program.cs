using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Infrastructure.DependencyInjection;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Users.CreateUser;
using UserManagement.Api.Features.Users.DeleteUser;
using UserManagement.Api.Features.Users.FindAllUsers;
using UserManagement.Api.Features.Users.FindOneUser;
using UserManagement.Api.Features.Users.UpdateUser;
using UserManagement.Domain.Users;
using UserManagement.Infrastructure.Persistence.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException($"'{JwtSettings.SectionName}' not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException($"'{JwtSettings.SectionName}:SecretKey' must have at least 32 characters.");

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
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<LoginHandler>();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapLogin();
app.MapCreateUser();
app.MapDeleteUser();
app.MapUpdateUser();
app.MapFindOneUser();
app.MapFindAllUser();

app.Run();
