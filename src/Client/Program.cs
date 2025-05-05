var assemblyName    = Assembly.GetExecutingAssembly().GetName().Name;
var appsettingsName = $"{assemblyName}.appsettings.json";

await Host.CreateDefaultBuilder(args)
          .ConfigureHostConfiguration(configHost =>
                                      {
                                          configHost.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                                          configHost.AddJsonFile(appsettingsName, true);
                                          configHost.AddEnvironmentVariables($"{assemblyName}_");
                                          configHost.AddCommandLine(args);
                                      })
          .ConfigureLogging(static logging =>
                            {
                                logging.AddConsole();
                                logging.AddDebug();
                            })
          .ConfigureServices(static services => services.AddHostedService<App>())
          .RunConsoleAsync()
          .ConfigureAwait(false);
