using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Core.Interfaces;

public interface ITransformService
{
    Task<IEnumerable<HechoVentaDTO>> TransformVentasAsync(IEnumerable<VentaDTO> ventas);
    Task<IEnumerable<HechoCompraDTO>> TransformComprasAsync(IEnumerable<CompraDTO> compras);
}

