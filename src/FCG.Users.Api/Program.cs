#region Using
using FCG.Users.Application.Interfaces;
using FCG.Users.Application.Services;
using FCG.Users.Application.UseCases.Users.CreateUser;
using FCG.Users.Application.UseCases.Users.DeleteUser;
using FCG.Users.Application.UseCases.Users.GetUserByEmail;
using FCG.Users.Application.UseCases.Users.GetUserById;
using FCG.Users.Application.UseCases.Users.ListUsers;
using FCG.Users.Application.UseCases.Users.Login;
using FCG.Users.Application.UseCases.Users.UpdateUser;
using FCG.Users.Domain.Interfaces;
using FCG.Users.Domain.Services;
using FCG.Users.Infra.Data;
using FCG.Users.Infra.Repositories;
using FCG.Users.Infra.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;
#endregion

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

#region persistence
// Infra: EF Core + MySQL
builder.Services.AddDbContext<UsersDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("UsersDb")
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__UsersDb")
             ?? throw new InvalidOperationException("ConnectionStrings:UsersDb not configured");

    opt.UseMySql(
        cs,
        ServerVersion.AutoDetect(cs),
        my => my.MigrationsAssembly(typeof(UsersDbContext).Assembly.FullName)
    );
});
#endregion

#region services
// Repositório: (Domain -> Infra)
builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();

// Domain: Services
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IUserCreationService, UserCreationService>();
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
builder.Services.AddScoped<IUserValidationService, UserValidationService>();


/* Application Services (JWT) */
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();


// Handlers (Application Use Cases)
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<LoginUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();
builder.Services.AddScoped<ListUsersHandler>();


// Swagger + HealthChecks
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FCG Users API",
        Version = "v1",
        Description = "Microserviço de Usuários do FIAP Cloud Games"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu_token_jwt}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHealthChecks();


// Authentication + Authorization (JWT)
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
#endregion



#region builder e pipeline
// Build
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#region helpers
static bool IsAdmin(ClaimsPrincipal user)
    => user.IsInRole("Admin");

static bool IsOwner(ClaimsPrincipal user, Guid routeUserId)
{
    var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? user.FindFirstValue(ClaimTypes.Name);

    return Guid.TryParse(sub, out var tokenUserId) && tokenUserId == routeUserId;
}
#endregion

// Pipeline
app.UseAuthentication();
app.UseAuthorization();
#endregion

#region endpoints
// TODO: WithSummary / WithDescription
#region Helthcheck, version
// Endpoints básicos (para teste rápido)
app.MapGet("/", () => new { service = "fcg-users-service", status = "ok" }).WithTags("Check");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).WithTags("Check");
app.MapGet("/version", () => new
{
    service = "fcg-users-service",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
}).WithTags("Check");
#endregion


app.MapPost("/api/users/register", async (
    CreateUserRequest req,
    CreateUserHandler handler,
    CancellationToken ct) =>
{
    var res = await handler.Handle(req, ct);
    return Results.Created($"/api/users/{res.Id}", res);
})
.WithTags("Anonymous")
.WithName("RegisterUser")
.WithSummary("Registro de novo usuario")
.WithDescription("Cria um novo usuario e retorna as informacoes incluindo o id guid")
.Accepts<CreateUserRequest>("application/json")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict)
.AllowAnonymous();

app.MapPost("/api/users/login", async (
    LoginUserRequest req,
    LoginUserHandler handler,
    IConfiguration cfg,
    CancellationToken ct) =>
{
    var jwt = cfg.GetSection("Jwt");
    var res = await handler.Handle(
        req,
        jwt["Issuer"]!, jwt["Audience"]!, jwt["Key"]!,
        TimeSpan.FromHours(2),
        ct);
    return Results.Ok(res);
})
.WithTags("Anonymous")
.WithName("LoginUser")
.WithSummary("Autentica e pega o JWT")
.WithDescription("Autentica as credencias do usuario e retorna o JWT para acessar rotas protegidas")
.Accepts<LoginUserRequest>("application/json")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.AllowAnonymous();


// ===== Admin =====
app.MapGet("/api/users", async (
    ListUsersHandler handler,
    CancellationToken ct) =>
{
    var res = await handler.Handle(new ListUsersRequest(), ct);
    return Results.Ok(res);
})
.WithTags("Admin")
.WithName("ListUsers")
.WithSummary("Lista todos os usuarios")
.WithDescription("Retorna paginado e completa lista de usuarios")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminOnly");

app.MapDelete("/api/users/{id:guid}", async (
    Guid id,
    DeleteUserHandler handler,
    CancellationToken ct) =>
{
    var res = await handler.Handle(new DeleteUserRequest(id), ct);
    return res.Deleted ? Results.NoContent() : Results.NotFound();
})
.WithTags("Admin")
.WithName("DeleteUser")
.WithSummary("Deleta usuario pelo id")
.WithDescription("Deleta pelo identificador guid precisa ser admin")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.RequireAuthorization("AdminOnly");


// ===== Self or Admin =====
app.MapGet("/api/users/{id:guid}", async (
    Guid id,
    GetUserByIdHandler handler,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    if (!(IsOwner(user, id) || IsAdmin(user)))
        return Results.Forbid();

    var res = await handler.Handle(new GetUserByIdRequest(id), ct);
    return Results.Ok(res);
})
.WithTags("Self or Admin")
.WithName("GetUserById")
.WithSummary("Get usuario pelo id")
.WithDescription("Retorna detalhes do usuario pelo id, precisa ser owner ou admin")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.Produces(StatusCodes.Status404NotFound)
.RequireAuthorization();

app.MapPut("/api/users/{id:guid}", async (
    Guid id,
    UpdateUserRequest body,
    UpdateUserHandler handler,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    if (!(IsOwner(user, id) || IsAdmin(user)))
        return Results.Forbid();

    var req = new UpdateUserRequest(id, body.Name, body.NewPassword);
    var res = await handler.Handle(req, ct);
    return Results.Ok(res);
})
.WithTags("Self or Admin")
.WithName("UpdateUser")
.WithSummary("Atualiza usuario pelo id")
.WithDescription("atualiza o perfil (nome e senha) precisa ser o owner ou admin")
.Accepts<UpdateUserRequest>("application/json")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status403Forbidden)
.Produces(StatusCodes.Status404NotFound)
.RequireAuthorization();

app.MapGet("/api/users/me", (ClaimsPrincipal user) =>
{
    if (!user.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    var response = new
    {
        Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
        Name = user.FindFirstValue(ClaimTypes.Name),
        Email = user.FindFirstValue(ClaimTypes.Email),
        Role = user.FindFirstValue(ClaimTypes.Role)
    };

    return Results.Ok(response);
})
.WithTags("Self or Admin")
.WithName("GetCurrentUser")
.WithSummary("Get o user atual autenticado")
.WithDescription("retorna nome email id role do usuario autenticado")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.RequireAuthorization();

#endregion



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
