using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;
using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Infrastructure.Services;

public class ExtractService : IExtractService
{
    private readonly string _connectionString;
    private readonly ILogger<ExtractService> _logger;

    public ExtractService(IConfiguration configuration, ILogger<ExtractService> logger)
    {
        _connectionString = configuration.GetConnectionString("ConnectionString") 
            ?? throw new ArgumentNullException(nameof(configuration), "ConnectionString is required");
        _logger = logger;
    }

    public async Task<IEnumerable<VentaDTO>> ExtractVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Ajustar fechaFin para incluir todo el día
        var fechaFinAjustada = fechaFin.Date.AddDays(1).AddSeconds(-1);
        
        // Primero verificar si hay datos en el rango
        var countQuery = @"
            SELECT COUNT(*) 
            FROM Ventas v
            WHERE v.Fecha_Venta BETWEEN @FechaInicio AND @FechaFinAjustada";
        
        var totalCount = await connection.QuerySingleAsync<int>(countQuery, new { FechaInicio = fechaInicio.Date, FechaFinAjustada = fechaFinAjustada });
        _logger.LogInformation("Found {TotalCount} ventas in date range {FechaInicio} to {FechaFin}", 
            totalCount, fechaInicio.Date, fechaFin.Date);
        
        if (totalCount == 0)
        {
            // Verificar el rango de fechas disponible en la BD
            var dateRangeQuery = @"
                SELECT MIN(Fecha_Venta) AS MinFecha, MAX(Fecha_Venta) AS MaxFecha, COUNT(*) AS Total
                FROM Ventas";
            var dateRange = await connection.QueryFirstOrDefaultAsync<(DateTime? MinFecha, DateTime? MaxFecha, int Total)>(dateRangeQuery);
            if (dateRange.MinFecha.HasValue)
            {
                _logger.LogWarning("No ventas found in range. Available dates: Min={MinFecha}, Max={MaxFecha}, Total={Total}", 
                    dateRange.MinFecha, dateRange.MaxFecha, dateRange.Total);
            }
            return Enumerable.Empty<VentaDTO>();
        }
        
        var query = @"
            SELECT 
                v.Id_Venta,
                v.Id_Cliente,
                v.Fecha_Venta,
                v.Cantidad_Total,
                dv.Id_Detalle_Venta,
                dv.Id_Productos,
                dv.Cantidad,
                dv.Precio_Unitario,
                dv.Linea_Total,
                c.Nombre AS Cliente_Nombre,
                c.Apellido AS Cliente_Apellido,
                c.Telefono AS Cliente_Telefono,
                c.Direccion AS Cliente_Direccion,
                c.Estado AS Cliente_Estado,
                c.Id_Empleados AS Cliente_Id_Empleados,
                p.Nombre_Producto AS Producto_Nombre_Producto,
                p.categoria_Id AS Producto_Categoria_Id,
                cat.Nombre_Categoria AS Categoria_Nombre,
                cat.Descripcion AS Categoria_Descripcion,
                e.Id_Empleados AS Empleado_Id,
                e.Nombre AS Empleado_Nombre,
                e.Apellido AS Empleado_Apellido
            FROM Ventas v
            INNER JOIN Detalle_Venta dv ON v.Id_Venta = dv.Id_Venta
            INNER JOIN Clientes c ON v.Id_Cliente = c.Id_Cliente
            INNER JOIN Productos p ON dv.Id_Productos = p.Id_Productos
            LEFT JOIN Categorias cat ON p.categoria_Id = cat.Id_Categoria
            LEFT JOIN Empleados e ON c.Id_Empleados = e.Id_Empleados
            WHERE v.Fecha_Venta >= @FechaInicio AND v.Fecha_Venta < @FechaFinAjustada
            ORDER BY v.Fecha_Venta";

        var ventas = await connection.QueryAsync<VentaDTO>(query, new { FechaInicio = fechaInicio.Date, FechaFinAjustada = fechaFinAjustada });
        
        _logger.LogInformation("Extracted {Count} ventas from {FechaInicio} to {FechaFin}", 
            ventas.Count(), fechaInicio.Date, fechaFin.Date);
        
        return ventas;
    }

    public async Task<IEnumerable<CompraDTO>> ExtractComprasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = new SqlConnection(_connectionString);
        
        // Ajustar fechaFin para incluir todo el día
        var fechaFinAjustada = fechaFin.Date.AddDays(1).AddSeconds(-1);
        
        // Primero verificar si hay datos en el rango
        var countQuery = @"
            SELECT COUNT(*) 
            FROM Compra c
            WHERE c.Fecha_Compra BETWEEN @FechaInicio AND @FechaFinAjustada";
        
        var totalCount = await connection.QuerySingleAsync<int>(countQuery, new { FechaInicio = fechaInicio.Date, FechaFinAjustada = fechaFinAjustada });
        _logger.LogInformation("Found {TotalCount} compras in date range {FechaInicio} to {FechaFin}", 
            totalCount, fechaInicio.Date, fechaFin.Date);
        
        if (totalCount == 0)
        {
            // Verificar el rango de fechas disponible en la BD
            var dateRangeQuery = @"
                SELECT MIN(Fecha_Compra) AS MinFecha, MAX(Fecha_Compra) AS MaxFecha, COUNT(*) AS Total
                FROM Compra";
            var dateRange = await connection.QueryFirstOrDefaultAsync<(DateTime? MinFecha, DateTime? MaxFecha, int Total)>(dateRangeQuery);
            if (dateRange.MinFecha.HasValue)
            {
                _logger.LogWarning("No compras found in range. Available dates: Min={MinFecha}, Max={MaxFecha}, Total={Total}", 
                    dateRange.MinFecha, dateRange.MaxFecha, dateRange.Total);
            }
            return Enumerable.Empty<CompraDTO>();
        }
        
        var query = @"
            SELECT 
                c.Id_compra,
                c.Id_Proveedor,
                c.Fecha_Compra,
                c.Cantidad_Total,
                c.Nombre_Negocio,
                dc.Id_Detalle_Compra,
                dc.Id_Productos,
                dc.Cantidad,
                dc.Precio_Unitario,
                dc.Linea_Total,
                p.Nombre_Negocio AS Proveedor_Nombre_Negocio,
                p.Nombre AS Proveedor_Nombre,
                p.Apellido AS Proveedor_Apellido,
                p.Telefono AS Proveedor_Telefono,
                p.Direccion AS Proveedor_Direccion,
                p.Estado AS Proveedor_Estado,
                pr.Nombre_Producto AS Producto_Nombre_Producto,
                pr.categoria_Id AS Producto_Categoria_Id,
                cat.Nombre_Categoria AS Categoria_Nombre,
                cat.Descripcion AS Categoria_Descripcion
            FROM Compra c
            INNER JOIN Detalle_Compra dc ON c.Id_compra = dc.Id_compra
            INNER JOIN Proveedor p ON c.Id_Proveedor = p.Id_Proveedor
            INNER JOIN Productos pr ON dc.Id_Productos = pr.Id_Productos
            LEFT JOIN Categorias cat ON pr.categoria_Id = cat.Id_Categoria
            WHERE c.Fecha_Compra >= @FechaInicio AND c.Fecha_Compra < @FechaFinAjustada
            ORDER BY c.Fecha_Compra";

        var compras = await connection.QueryAsync<CompraDTO>(query, new { FechaInicio = fechaInicio.Date, FechaFinAjustada = fechaFinAjustada });
        
        _logger.LogInformation("Extracted {Count} compras from {FechaInicio} to {FechaFin}", 
            compras.Count(), fechaInicio.Date, fechaFin.Date);
        
        return compras;
    }
}