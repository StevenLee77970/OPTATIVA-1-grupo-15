using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;
using PlasticHouseETL.Infrastructure.Configuration;
using PlasticHouseETL.Infrastructure.Services;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/etl-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("ETL Application Starting");

    // Configurar configuración
    var basePath = Directory.GetCurrentDirectory();
    var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    
    // Si ejecutamos desde bin/Debug, buscar en el directorio del proyecto
    if (assemblyPath != null && assemblyPath.Contains("bin"))
    {
        var projectPath = Path.Combine(assemblyPath, "..", "..", "..");
        if (Directory.Exists(projectPath))
        {
            var settingsPath = Path.Combine(projectPath, "appsettings.json");
            if (File.Exists(settingsPath))
            {
                basePath = projectPath;
            }
        }
    }
    
    var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Configurar servicios
    var services = new ServiceCollection();
    
    // Configuración
    services.AddSingleton<IConfiguration>(configuration);
    
    // ETLSettings
    var etlSettings = new ETLSettings();
    configuration.GetSection("ETLSettings").Bind(etlSettings);
    services.AddSingleton(etlSettings);
    
    // Logging
    services.AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddSerilog();
    });
    
    // Registrar servicios ETL
    services.AddScoped<IExtractService, ExtractService>();
    services.AddScoped<IDimensionService, DimensionService>();
    services.AddScoped<ITransformService, TransformService>();
    services.AddScoped<ILoadService, LoadService>();
    services.AddScoped<IETLService, ETLService>();

    var serviceProvider = services.BuildServiceProvider();
    
    var etlService = serviceProvider.GetRequiredService<IETLService>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    // Determinar modo de ejecución
    var commandLineArgs = Environment.GetCommandLineArgs();
    var mode = commandLineArgs.Length > 1 ? commandLineArgs[1].ToLower() : "incremental";

    logger.LogInformation("ETL execution mode: {Mode}", mode);

    // Ejecutar ETL según el modo
    switch (mode)
    {
        case "full":
            logger.LogInformation("Executing FULL ETL process");
            await etlService.ExecuteFullAsync();
            break;
        case "incremental":
        default:
            logger.LogInformation("Executing INCREMENTAL ETL process");
            await etlService.ExecuteIncrementalAsync();
            break;
    }

    logger.LogInformation("ETL process completed successfully");
    Log.Information("ETL Application Completed Successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "ETL Application Failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

