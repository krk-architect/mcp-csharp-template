var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(static consoleLogOptions =>
                           {
                               // Configure all logs to go to stderr
                               consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
                           });

builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly(Assembly.GetAssembly(typeof(Core.Tools.DateTool)));

await builder.Build()
             .RunAsync()
             .ConfigureAwait(false);
