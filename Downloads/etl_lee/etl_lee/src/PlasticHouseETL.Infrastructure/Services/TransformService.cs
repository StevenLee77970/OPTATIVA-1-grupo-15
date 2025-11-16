using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;
using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Infrastructure.Services;

public class TransformService : ITransformService
{
    private readonly IDimensionService _dimensionService;
    private readonly ILogger<TransformService> _logger;

    public TransformService(IDimensionService dimensionService, ILogger<TransformService> logger)
    {
        _dimensionService = dimensionService;
        _logger = logger;
    }

    public async Task<IEnumerable<HechoVentaDTO>> TransformVentasAsync(IEnumerable<VentaDTO> ventas)
    {
        var hechoVentas = new List<HechoVentaDTO>();

        foreach (var venta in ventas)
        {
            try
            {
                var tiempoPK = await _dimensionService.GetOrCreateTiempoIdAsync(venta.Fecha_Venta);
                var clientePK = await _dimensionService.GetOrCreateClienteIdAsync(venta.Id_Cliente);
                var productoPK = await _dimensionService.GetOrCreateProductoIdAsync(venta.Id_Productos);
                var empleadoPK = await _dimensionService.GetOrCreateEmpleadoIdAsync(venta.Cliente_Id_Empleados);
                var categoriaPK = await _dimensionService.GetOrCreateCategoriaIdAsync(venta.Producto_Categoria_Id ?? 0);

                var hechoVenta = new HechoVentaDTO
                {
                    ClientePK = clientePK,
                    ProductoPK = productoPK,
                    EmpleadoPK = empleadoPK,
                    TiempoPK = tiempoPK,
                    CategoriaPK = categoriaPK,
                    Ventas_CantidadVendida = venta.Cantidad,
                    Ventas_PrecioUnitario = venta.Precio_Unitario,
                    Ventas_SubtotalLinea = venta.Linea_Total,
                    Ventas_DescuentoLinea = 0, // No hay descuento en el esquema operacional
                    Ventas_TotalLinea = venta.Linea_Total,
                    Ventas_Id_FacturaOriginal = venta.Id_Venta,
                    Ventas_Id_DetalleOriginal = venta.Id_Detalle_Venta
                };

                hechoVentas.Add(hechoVenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming venta {VentaId}", venta.Id_Venta);
            }
        }

        _logger.LogInformation("Transformed {Count} ventas to {HechoCount} HechoVenta records", 
            ventas.Count(), hechoVentas.Count());

        return hechoVentas;
    }

    public async Task<IEnumerable<HechoCompraDTO>> TransformComprasAsync(IEnumerable<CompraDTO> compras)
    {
        var hechoCompras = new List<HechoCompraDTO>();

        foreach (var compra in compras)
        {
            try
            {
                var tiempoPK = await _dimensionService.GetOrCreateTiempoIdAsync(compra.Fecha_Compra);
                var proveedorPK = await _dimensionService.GetOrCreateProveedorIdAsync(compra.Id_Proveedor);
                var productoPK = await _dimensionService.GetOrCreateProductoIdAsync(compra.Id_Productos);
                var categoriaPK = await _dimensionService.GetOrCreateCategoriaIdAsync(compra.Producto_Categoria_Id ?? 0);
                // Para compras, el empleado puede ser null o necesitar lógica adicional
                int? empleadoPK = null;

                var hechoCompra = new HechoCompraDTO
                {
                    ProveedorPK = proveedorPK,
                    ProductoPK = productoPK,
                    EmpleadoPK = empleadoPK,
                    TiempoPK = tiempoPK,
                    CategoriaPK = categoriaPK,
                    Compras_CantidadComprada = compra.Cantidad,
                    Compras_PrecioUnitario = compra.Precio_Unitario,
                    Compras_SubtotalLinea = compra.Linea_Total,
                    Compras_DescuentoLinea = 0, // No hay descuento en el esquema operacional
                    Compras_TotalLinea = compra.Linea_Total,
                    Compras_Id_CompraOriginal = compra.Id_compra,
                    Compras_Id_DetalleOriginal = compra.Id_Detalle_Compra
                };

                hechoCompras.Add(hechoCompra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming compra {CompraId}", compra.Id_compra);
            }
        }

        _logger.LogInformation("Transformed {Count} compras to {HechoCount} HechoCompra records", 
            compras.Count(), hechoCompras.Count());

        return hechoCompras;
    }
}

