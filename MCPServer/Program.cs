using MCPServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpcionesApi>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<ClienteApi>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithTools<HerramientasDragonBall>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();