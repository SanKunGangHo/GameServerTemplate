using System.Security.Claims;
using ChatServer.Database;
using ChatServer.Repositories;
using ChatServer.Services;
using ChatServer.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//연결 문자열 가져오기
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

//DI 설정 (의존성 주입)
builder.Services.AddSingleton(new DatabaseInitializer(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<WebSocketHandler>();

//JWT 인증 설정
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["token"];
                if (!string.IsNullOrEmpty(token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

app.Map("/ws/{channelId}", async (HttpContext ctx, WebSocketHandler handler, long channelId) =>
{    
    if (!ctx.WebSockets.IsWebSocketRequest)
    {
        ctx.Response.StatusCode = 400;
        return;
    }

    var userIdClaim = ctx.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
    if (userIdClaim is null)
    {
        ctx.Response.StatusCode = 401;
        return;
    }

    var userId = long.Parse(userIdClaim.Value);
    var socket = await ctx.WebSockets.AcceptWebSocketAsync();
    await handler.HandleAsync(socket, userId, channelId);
});

app.MapPost("/auth/register", async (AuthService auth, RegisterRequest req) =>
{
    var user = await auth.RegisterAsync(req.Username, req.Password);
    return Results.Ok(new { user.Id, user.Username });
});

app.MapPost("/auth/login", async (AuthService auth, LoginRequest req) =>
{
    var token = await auth.LoginAsync(req.Username, req.Password);
    if (token is null) return Results.Unauthorized();
    return Results.Ok(new { token });
});

app.Run();    
    
record RegisterRequest(string Username, string Password);
record LoginRequest(string Username, string Password);