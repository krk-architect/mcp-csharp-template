var assemblyName    = Assembly.GetExecutingAssembly().GetName().Name;
var appsettingsName = $"{assemblyName}.appsettings.json";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
       .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
       .AddJsonFile(appsettingsName, true, true)
       .AddEnvironmentVariables("McpCsharpTemplate_HttpServer_");

builder.Logging
       .ClearProviders()
       .AddConsole()
       .AddDebug()
       .AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services
       .AddMcpServer()
       .WithHttpTransport()
       .WithToolsFromAssembly(Assembly.GetAssembly(typeof(Core.Tools.DateTool)));

var app = builder.Build();

app.MapMcp();

app.Run();
