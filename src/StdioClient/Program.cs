var assemblyName    = Assembly.GetExecutingAssembly().GetName().Name;
var appsettingsName = $"{assemblyName}.appsettings.json";

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHostConfiguration(configHost =>
                                   {
                                       configHost.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                                       configHost.AddJsonFile(appsettingsName, true);
                                       configHost.AddEnvironmentVariables($"{assemblyName}_");
                                       configHost.AddCommandLine(args);
                                   });

builder.ConfigureLogging(static logging =>
                         {
                             logging.AddConsole();
                             logging.AddDebug();
                         });

builder.ConfigureServices(static services => services.AddHostedService<App>());

await builder.RunConsoleAsync()
             .ConfigureAwait(false);
