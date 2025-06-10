namespace Client;

[ExcludeFromCodeCoverage]
public partial class App : IHostedService
{
    public App(ILogger<App> logger, IHostApplicationLifetime appLifetime)
    {
        Logger      = logger;
        AppLifetime = appLifetime;
    }

    private ILogger<App>             Logger      { get; }
    private IHostApplicationLifetime AppLifetime { get; }
    private int                      ExitCode    { get; set; }

    private async Task RunWithArgsAsync(string name, string command, List<string> arguments)
    {
        try
        {
            var clientTransport = new StdioClientTransport(new()
                                                           {
                                                               Name      = name
                                                             , Command   = command
                                                             , Arguments = arguments
                                                           });

            var client = await McpClientFactory.CreateAsync(clientTransport).ConfigureAwait(false);
            var tools  = await client.GetMcpToolsAsync().ConfigureAwait(false);

            while (true)
            {
                var selection = PromptUserForToolSelection(tools);

                if (selection == 0)
                {
                    break;
                }

                var selectedTool = tools[selection - 1];
                var toolSchema   = selectedTool.ParseToolSchema();

                PromptUserForPropertyValues(toolSchema);

                var callToolResponse = await client.CallToolAsync(selectedTool.Name
                                                                , toolSchema.GetCallToolArguments()
                                                                , cancellationToken: CancellationToken.None)
                                                   .ConfigureAwait(false);

                DisplayToolResponse(callToolResponse);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unhandled exception!");
            ExitCode = 2;
            return;
        }

        ExitCode = 0;
    }

    private static string ResolvePath(string path)
    {
        var devOnlyFilePath = Path.Combine(AppContext.BaseDirectory, "Client.runtimeconfig.json");

        var baseDir = !File.Exists(devOnlyFilePath)
                          ? AppContext.BaseDirectory                                                  // published or copied with Files/
                          : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../")); // dev context

        return Path.IsPathRooted(path)
                   ? path
                   : Path.GetFullPath(Path.Combine(baseDir, path.TrimStart('.', '/', '\\')));
    }

    #region IHostedService

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Starting with arguments: {Args}", string.Join(" ", Environment.GetCommandLineArgs()));

        AppLifetime.ApplicationStarted.Register(() => _ = HandleApplicationStartedAsync());

        return Task.CompletedTask;

        async Task HandleApplicationStartedAsync()
        {
            try
            {
                _ = await CreateRootCommand().InvokeAsync(Environment.GetCommandLineArgs()[1..]).ConfigureAwait(false); // Skip the first argument (executable path)
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Unhandled exception!");
                ExitCode = 1;
            }
            finally
            {
                AppLifetime.StopApplication();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Exiting with return code: {ExitCode}", ExitCode);

        Environment.ExitCode = ExitCode;

        return Task.CompletedTask;
    }

    #endregion
}
