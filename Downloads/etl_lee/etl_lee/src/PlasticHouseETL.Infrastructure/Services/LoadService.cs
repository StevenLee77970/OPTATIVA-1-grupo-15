using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;
using PlasticHouseETL.Infrastructure.Configuration;
using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Infrastructure.Services;

public class LoadService : ILoadService
{
    private readonly string _connectionString;
    private readonly ILogger<LoadService> _logger;
    private readonly int _batchSize;

    public LoadService(IConfiguration configuration, ILogger<LoadService> logger, ETLSettings settings)
    {
        _connectionString = configuration.GetConnectionString("DataWarehouseDB") 
            ?? throw new ArgumentNullException(nameof(configuration), "DataWarehouseDB connection string is required");
        _logger = logger;
        _batchSize = settings.BatchSize;
    }

    public async Task LoadVentasAsync(IEnumerable<HechoVentaDTO> hechoVentas)
    {
        if (!hechoVentas.Any())
        {
            _logger.LogInformation("No ventas to load");
            return;
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var batches = hechoVentas.Chunk(_batchSize);
        int batchNumber = 0;

        foreach (var batch in batches)
        {
            batchNumber++;
            
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var hechoVenta in batch)
                {
                    var mergeQuery = @"
                        MERGE FACT_Ventas AS target
                        USING (SELECT 
                            @ClientePK AS ClientePK,
                            @ProductoPK AS ProductoPK,
                            @EmpleadoPK AS EmpleadoPK,
                            @TiempoPK AS TiempoPK,
                            @CategoriaPK AS CategoriaPK,
                            @Ventas_Id_DetalleOriginal AS Ventas_Id_DetalleOriginal
                        ) AS source
                        ON target.ClientePK = source.ClientePK
                            AND target.ProductoPK = source.ProductoPK
                            AND target.TiempoPK = source.TiempoPK
                            AND target.Ventas_Id_DetalleOriginal = source.Ventas_Id_DetalleOriginal
                        WHEN MATCHED THEN
                            UPDATE SET
                                Ventas_CantidadVendida = Ventas_CantidadVendida + @Ventas_CantidadVendida,
                                Ventas_TotalLinea = Ventas_TotalLinea + @Ventas_TotalLinea,
                                ETLLoad = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT (
                                ClientePK, ProductoPK, EmpleadoPK, TiempoPK, CategoriaPK,
                                Ventas_CantidadVendida, Ventas_PrecioUnitario, Ventas_SubtotalLinea,
                                Ventas_DescuentoLinea, Ventas_TotalLinea,
                                Ventas_Id_FacturaOriginal, Ventas_Id_DetalleOriginal, ETLLoad
                            )
                            VALUES (
                                @ClientePK, @ProductoPK, @EmpleadoPK, @TiempoPK, @CategoriaPK,
                                @Ventas_CantidadVendida, @Ventas_PrecioUnitario, @Ventas_SubtotalLinea,
                                @Ventas_DescuentoLinea, @Ventas_TotalLinea,
                                @Ventas_Id_FacturaOriginal, @Ventas_Id_DetalleOriginal, GETDATE()
                            );";
                    
                    await connection.ExecuteAsync(mergeQuery, hechoVenta, transaction);
                }
                
                transaction.Commit();
                _logger.LogInformation("Loaded batch {BatchNumber} with {Count} FACT_Ventas records", batchNumber, batch.Length);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error loading batch {BatchNumber} of FACT_Ventas", batchNumber);
                throw;
            }
        }

        _logger.LogInformation("Successfully loaded {TotalCount} FACT_Ventas records", hechoVentas.Count());
    }

    public async Task LoadComprasAsync(IEnumerable<HechoCompraDTO> hechoCompras)
    {
        if (!hechoCompras.Any())
        {
            _logger.LogInformation("No compras to load");
            return;
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var batches = hechoCompras.Chunk(_batchSize);
        int batchNumber = 0;

        foreach (var batch in batches)
        {
            batchNumber++;
            
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var hechoCompra in batch)
                {
                    var mergeQuery = @"
                        MERGE FACT_Compras AS target
                        USING (SELECT 
                            @ProveedorPK AS ProveedorPK,
                            @ProductoPK AS ProductoPK,
                            @EmpleadoPK AS EmpleadoPK,
                            @TiempoPK AS TiempoPK,
                            @CategoriaPK AS CategoriaPK,
                            @Compras_Id_DetalleOriginal AS Compras_Id_DetalleOriginal
                        ) AS source
                        ON target.ProveedorPK = source.ProveedorPK
                            AND target.ProductoPK = source.ProductoPK
                            AND target.TiempoPK = source.TiempoPK
                            AND target.Compras_Id_DetalleOriginal = source.Compras_Id_DetalleOriginal
                        WHEN MATCHED THEN
                            UPDATE SET
                                Compras_CantidadComprada = Compras_CantidadComprada + @Compras_CantidadComprada,
                                Compras_TotalLinea = Compras_TotalLinea + @Compras_TotalLinea,
                                ETLLoad = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT (
                                ProveedorPK, ProductoPK, EmpleadoPK, TiempoPK, CategoriaPK,
                                Compras_CantidadComprada, Compras_PrecioUnitario, Compras_SubtotalLinea,
                                Compras_DescuentoLinea, Compras_TotalLinea,
                                Compras_Id_CompraOriginal, Compras_Id_DetalleOriginal, ETLLoad
                            )
                            VALUES (
                                @ProveedorPK, @ProductoPK, @EmpleadoPK, @TiempoPK, @CategoriaPK,
                                @Compras_CantidadComprada, @Compras_PrecioUnitario, @Compras_SubtotalLinea,
                                @Compras_DescuentoLinea, @Compras_TotalLinea,
                                @Compras_Id_CompraOriginal, @Compras_Id_DetalleOriginal, GETDATE()
                            );";
                    
                    await connection.ExecuteAsync(mergeQuery, hechoCompra, transaction);
                }
                
                transaction.Commit();
                _logger.LogInformation("Loaded batch {BatchNumber} with {Count} FACT_Compras records", batchNumber, batch.Length);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error loading batch {BatchNumber} of FACT_Compras", batchNumber);
                throw;
            }
        }

        _logger.LogInformation("Successfully loaded {TotalCount} FACT_Compras records", hechoCompras.Count());
    }
}

