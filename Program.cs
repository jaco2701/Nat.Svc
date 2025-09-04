using Nat.Svc;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<NatSetting>(context.Configuration.GetSection("NatSetting"));
        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .ConfigureLogging(logging =>
    {
        logging.AddEventLog();
    })
    .Build();

await host.RunAsync();
