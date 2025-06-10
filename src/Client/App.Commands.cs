namespace Client;

public partial class App
{
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
                            , CreateSseCommand(   nameOption, argumentsOption)
                            , CreateHttpCommand(  nameOption, argumentsOption)
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
                               var projectPath = ResolvePath(project); // allow for relative paths

                               await RunWithStdioAsync(name
                                                     , "dotnet"
                                                     , [
                                                           "run"
                                                         , "--project"
                                                         , projectPath
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
                               await RunWithStdioAsync(name
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
                               await RunWithStdioAsync(name
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
                               await RunWithStdioAsync(name
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
                               await RunWithStdioAsync(name
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

    private Command CreateSseCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("sse", "Connect to an MCP server using SSE transport (legacy compatibility)");

        var urlOption = CreateOption<string>("--url"
                                           , "The URL of the SSE MCP server endpoint"
                                           , "-u"
                                           , true);

        command.Add(nameOption);
        command.Add(urlOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, url, arguments) =>
                           {
                               await RunWithSseAsync(name, url, arguments).ConfigureAwait(false);
                           }
                         , nameOption
                         , urlOption
                         , argumentsOption);

        return command;
    }

    private Command CreateHttpCommand(Option<string> nameOption, Option<List<string>> argumentsOption)
    {
        var command = new Command("http", "Connect to an MCP server using HTTP transport (modern servers)");

        var urlOption = CreateOption<string>("--url"
                                           , "The base URL of the HTTP MCP server"
                                           , "-u"
                                           , true);

        command.Add(nameOption);
        command.Add(urlOption);
        command.Add(argumentsOption);

        command.SetHandler(async (name, url, arguments) =>
                           {
                               await RunWithHttpAsync(name, url, arguments).ConfigureAwait(false);
                           }
                         , nameOption
                         , urlOption
                         , argumentsOption);

        return command;
    }
}
