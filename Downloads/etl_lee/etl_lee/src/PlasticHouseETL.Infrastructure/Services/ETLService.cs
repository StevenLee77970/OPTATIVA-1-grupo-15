using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;

namespace PlasticHouseETL.Infrastructure.Services;

public class ETLService : IETLService
{
    private readonly IExtractService _extractService;
    private readonly ITransformService _transformService;
    private readonly ILoadService _loadService;
    private readonly ILogger<ETLService> _logger;

    public ETLService(
        IExtractService extractService,
        ITransformService transformService,
        ILoadService loadService,
        ILogger<ETLService> logger)
    {
        _extractService = extractService;
        _transformService = transformService;
        _loadService = loadService;
        _logger = logger;
    }

    public async Task ExecuteAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        _logger.LogInformation("Starting ETL process from {FechaInicio} to {FechaFin}", fechaInicio, fechaFin);
        
        try
        {
            // Extract
            _logger.LogInformation("Step 1: Extract - Starting data extraction");
            var ventas = await _extractService.ExtractVentasAsync(fechaInicio, fechaFin);
            var compras = await _extractService.ExtractComprasAsync(fechaInicio, fechaFin);
            _logger.LogInformation("Step 1: Extract - Completed. Ventas: {VentasCount}, Compras: {ComprasCount}",
                ventas.Count(), compras.Count());

            // Transform
            _logger.LogInformation("Step 2: Transform - Starting data transformation");
            var hechoVentas = await _transformService.TransformVentasAsync(ventas);
            var hechoCompras = await _transformService.TransformComprasAsync(compras);
            _logger.LogInformation("Step 2: Transform - Completed. HechoVentas: {HechoVentasCount}, HechoCompras: {HechoComprasCount}",
                hechoVentas.Count(), hechoCompras.Count());

            // Load
            _logger.LogInformation("Step 3: Load - Starting data loading");
            await _loadService.LoadVentasAsync(hechoVentas);
            await _loadService.LoadComprasAsync(hechoCompras);
            _logger.LogInformation("Step 3: Load - Completed");

            _logger.LogInformation("ETL process completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ETL process");
            throw;
        }
    }

    public async Task ExecuteFullAsync()
    {
        // Ejecutar ETL completo desde 2020 hasta hoy
        var fechaInicio = new DateTime(2020, 1, 1);
        var fechaFin = DateTime.Now;
        await ExecuteAsync(fechaInicio, fechaFin);
    }

    public async Task ExecuteIncrementalAsync()
    {
        // Ejecutar ETL incremental (últimas 24 horas)
        var fechaInicio = DateTime.Now.AddDays(-1);
        var fechaFin = DateTime.Now;
        await ExecuteAsync(fechaInicio, fechaFin);
    }
}

