namespace Client;

public class McpToolSchema
{
    public string                Title       { get; set; } = string.Empty;
    public string                Description { get; set; } = string.Empty;
    public string                Type        { get; set; } = "object";
    public List<McpToolProperty> Properties  { get; set; } = [];
}

public class McpToolProperty
{
    public string  Name     { get; set; } = null!;
    public string  Type     { get; set; } = null!;
    public bool    Required { get; set; }
    public object? Value    { get; set; }
}

[ExcludeFromCodeCoverage]
public class App : IHostedService
{
    public App(ILogger<App> logger, IHostApplicationLifetime appLifetime)
    {
        Logger      = logger;
        AppLifetime = appLifetime;
    }

    private ILogger<App>             Logger      { get; }
    private IHostApplicationLifetime AppLifetime { get; }
    private int                      ExitCode    { get; set; }

    private static void DisplayTools(IList<McpClientTool> tools)
    {
        Console.WriteLine("\n\nAvailable tools:");
        Console.WriteLine(new string('=', 16));
        Console.WriteLine("0. Exit");

        var maxNameLength = tools.Max(static tool => tool.Name.Length);

        for (var i = 0; i < tools.Count; i++)
        {
            var tool = tools[i];

            Console.WriteLine($"{i + 1}. {tool.Name.PadRight(maxNameLength, ' ')} - {tool.Description}");
        }
    }

    private static void DisplayResponse(CallToolResponse response)
    {
        Console.Write("\nResponse: ");
        foreach (var content in response.Content.Where(static c => c.Type == "text"))
        {
            Console.WriteLine(content.Text);
        }
    }

    private static int PromptUserForToolSelection(IList<McpClientTool> tools)
    {
        const string prompt = "\nSelect a tool to execute (enter the number): ";

        DisplayTools(tools);

        Console.Write(prompt);
        var selection = -1;
        while (selection < 0 || selection > tools.Count)
        {
            if (int.TryParse(Console.ReadLine(), out var input))
            {
                selection = input;
                if (selection >= 0 && selection <= tools.Count)
                {
                    continue;
                }

                Console.WriteLine($"Please enter a number between 0 and {tools.Count}");
                Console.Write(prompt);
            }
            else
            {
                Console.WriteLine("Please enter a valid number");
                Console.Write(prompt);
            }
        }

        return selection;
    }

    private static void PromptUserForPropertyValue(McpToolProperty property)
    {
        do
        {
            Console.Write($"{property.Name} [{(property.Required ? "Required" : "Optional")}]: ");

            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                property.Value = property.Type switch
                                 {
                                     "string"  => value
                                   , "number"  => double.TryParse(value, out var numberValue) ? numberValue : null
                                   , "boolean" => bool.TryParse(value, out var boolValue) ? boolValue : null
                                   , _         => value
                                 };

                break;
            }

            if (!property.Required)
            {
                break;
            }

            Console.WriteLine($"The parameter '{property.Name}' is required. Please provide a value.");
        } while (true);
    }

    private static McpToolSchema ParseToolSchema(McpClientTool tool)
    {
        var jsonSchema = tool.JsonSchema;

        var toolSchema = new McpToolSchema();

        if (jsonSchema.TryGetProperty("title", out var titleElement))
        {
            toolSchema.Title = titleElement.GetString() ?? string.Empty;
        }

        if (jsonSchema.TryGetProperty("description", out var descriptionElement))
        {
            toolSchema.Description = descriptionElement.GetString() ?? string.Empty;
        }

        if (jsonSchema.TryGetProperty("type", out var typeElement))
        {
            toolSchema.Type = typeElement.GetString() ?? "object";
        }

        // Check if the schema contains a "properties" object which defines parameters
        if (!jsonSchema.TryGetProperty("properties", out var propertiesElement) || propertiesElement.ValueKind != JsonValueKind.Object)
        {
            return toolSchema;
        }

        var requiredProperties = new HashSet<string>();
        if (jsonSchema.TryGetProperty("required", out var requiredArray) && requiredArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var requiredProperty in requiredArray.EnumerateArray())
            {
                if (requiredProperty.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var requiredPropertyName = requiredProperty.GetString();
                if (requiredPropertyName is not null)
                {
                    requiredProperties.Add(requiredPropertyName);
                }
            }
        }

        foreach (var jsonProperty in propertiesElement.EnumerateObject())
        {
            var property = new McpToolProperty
                           {
                               Name = jsonProperty.Name
                           };

            if (jsonProperty.Value.TryGetProperty("type", out var propertyTypeElement))
            {
                property.Type = propertyTypeElement.GetString() ?? "string";
            }

            if (requiredProperties.Contains(property.Name))
            {
                property.Required = true;
            }

            toolSchema.Properties.Add(property);
        }

        toolSchema.Properties = [.. toolSchema.Properties.OrderBy(static p => p.Name)];

        return toolSchema;
    }

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
            var tools  = await client.ListToolsAsync().ConfigureAwait(false);

            while (true)
            {
                var selection = PromptUserForToolSelection(tools);

                if (selection == 0)
                {
                    break;
                }

                var selectedTool = tools[selection - 1];
                var toolSchema   = ParseToolSchema(selectedTool);

                if (toolSchema.Properties.Count > 0)
                {
                    Console.WriteLine("\nEnter input values:");
                    Console.WriteLine(new string('=', 19));
                    foreach (var property in toolSchema.Properties)
                    {
                        PromptUserForPropertyValue(property);
                    }
                }

                var parameters = toolSchema.Properties.ToDictionary(static p => p.Name, static p => p.Value);

                var callToolResponse = await client.CallToolAsync(selectedTool.Name
                                                                , parameters
                                                                , cancellationToken: CancellationToken.None)
                                                   .ConfigureAwait(false);

                DisplayResponse(callToolResponse);
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

    #region Commands and Options

    private RootCommand CreateRootCommand()
    {
        var nameOption      = CreateNameOption();
        var argumentsOption = CreateArgumentsOption();

        var rootCommand = new RootCommand("MCP Client")
                          {
                              CreateDotNetCommand(nameOption, argumentsOption)
                            , CreateNpxCommand(   nameOption, argumentsOption)
                            , CreateNodeCommand(  nameOption, argumentsOption)
                            , CreateUvxCommand(   nameOption, argumentsOption)
                            , CreatePythonCommand(nameOption, argumentsOption)
                          };

        return rootCommand;
    }

    private static Option<T> CreateOption<T>(string  name
                                           , string  description
                                           , string? alias                          = null
                                           , bool    isRequired                     = false
                                           , bool    allowMultipleArgumentsPerToken = false)
    {
        var option = new Option<T>(name, description)
                     {
                         IsRequired                     = isRequired
                       , AllowMultipleArgumentsPerToken = allowMultipleArgumentsPerToken
                     };

        if (alias is not null)
        {
            option.AddAlias(alias);
        }

        return option;
    }

    private static Option<string> CreateNameOption()
        => CreateOption<string>("--name"
                              , "The MCP server name"
                              , "-n"
                              , true);

    private static Option<List<string>> CreateArgumentsOption()
        => CreateOption<List<string>>("--arguments"
                                    , "Additional arguments to provide to the MCP server tool"
                                    , "-a"
                                    , false
                                    , true);

    private Command CreateDotNetCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("dotnet", "Run a .NET MCP server");

        var projectPathOption = CreateOption<string>("--project"
                                                   , "The path to the .NET project file path"
                                                   , "-p"
                                                   , true);

        command.Add(nameOption);
        command.Add(projectPathOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, project, arguments) =>
                           {
                               await RunWithArgsAsync(name
                                                    , "dotnet"
                                                    , [
                                                          "run"
                                                        , "--project"
                                                        , project
                                                        , ..arguments
                                                      ])
                                  .ConfigureAwait(false);
                           }
                         , nameOption
                         , projectPathOption
                         , argumentsOption);

        return command;
    }

    private Command CreateNpxCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("npx", "Run a Node.js MCP server with npx");

        var packageNameOption = CreateOption<string>("--package"
                                                   , "The name of the NPM package to run"
                                                   , "-p"
                                                   , true);

        command.Add(nameOption);
        command.Add(packageNameOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, package, arguments) =>
                           {
                               await RunWithArgsAsync(name
                                                    , "npx"
                                                    , [
                                                          "-y"
                                                        , package
                                                        , ..arguments
                                                      ])
                                  .ConfigureAwait(false);
                           }
                         , nameOption
                         , packageNameOption
                         , argumentsOption);

        return command;
    }

    private Command CreateNodeCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("node", "Run a Node.js MCP server with a path to node.exe");

        var filePathOption = CreateOption<string>("--file"
                                                , "The path to the JavaScript file to run"
                                                , "-f"
                                                , true);

        command.Add(nameOption);
        command.Add(filePathOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, jsFilePath, arguments) =>
                           {
                               await RunWithArgsAsync(name
                                                    , "node.exe"
                                                    , [
                                                          jsFilePath
                                                        , ..arguments
                                                      ])
                                  .ConfigureAwait(false);
                           }
                         , nameOption
                         , filePathOption
                         , argumentsOption);

        return command;
    }

    private Command CreateUvxCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("uvx", "Run a Python MCP server with uvx");

        var packageOption = CreateOption<string>("--package"
                                               , "The name of the Python package to run"
                                               , "-p"
                                               , true);

        command.Add(nameOption);
        command.Add(packageOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, package, arguments) =>
                           {
                               await RunWithArgsAsync(name
                                                    , "uvx"
                                                    , [
                                                          package
                                                        , ..arguments
                                                      ])
                                  .ConfigureAwait(false);
                           }
                         , nameOption
                         , packageOption
                         , argumentsOption);

        return command;
    }

    private Command CreatePythonCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("python", "Run a Python MCP server with a path to python.exe");

        var pythonPackageOption = CreateOption<string>("--package"
                                                     , "The name of the Python package to run"
                                                     , "-p"
                                                     , true);

        command.Add(nameOption);
        command.Add(pythonPackageOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, jsFilePath, arguments) =>
                           {
                               await RunWithArgsAsync(name
                                                    , "python.exe"
                                                    , [
                                                          jsFilePath
                                                        , ..arguments
                                                      ])
                                  .ConfigureAwait(false);
                           }
                         , nameOption
                         , pythonPackageOption
                         , argumentsOption);

        return command;
    }

    #endregion

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
