using AspNetCoreRateLimit;
using CQRS;
using CQRS.CQRS.Commands;
using CQRS.CQRS.Queries;
using CQRS.Database;
using CQRS.Model;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Repository>();
builder.Services.AddSingleton<SecurityService>();

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(Program).Assembly));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                //Endpoint = "GET:/employee/getAllEmployees",
                Endpoint = "*",
                Period = "20s",
                Limit = 2,
            }
        };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero,
    };
});

var app = builder.Build();

app.UseIpRateLimiting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//******APIs******

app.MapPost("/login", async (SecurityService security, LogIn login) =>
{
    var user = security.Authenticate(login);
    if (user != null)
    {
        var token = security.GenerateToken(user);
        return Results.Ok(token);
    }
    return Results.NotFound("user not found");
});

app.MapPost("/is-valid", async (HttpContext ctx, SecurityService security, HttpRequest request) =>
{
    //var token = ctx.Request.Headers[HeaderNames.Authorization];
    //var tokenString = token.ToString();

    //var token2 = ctx.Request.Headers.Authorization;

    //var validation = new JwtSecurityTokenHandler().ValidateToken(tokenString);

    var identity = ctx.User.Identity as ClaimsIdentity;
    var result = security.ValidateToken(identity);
    return Results.Ok(result);
});

app.MapGet("/", () => "Hello World!");


app.MapGet("/todo/{id}", async (IMediator mediator, int id) =>
{
    var query = new GetTodoByIdQuery(id);
    var response = await mediator.Send(query);
    return response;
});

app.MapPost("/todo", async (IMediator mediator, Todo data) =>
{
    IRequest<CreateTodoResponse> command = new CreateTodoCommand(data.Name);
    CreateTodoResponse response = await mediator.Send(command);
    return response;
});

app.Run();