using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Core.Interfaces;

public interface ILoadService
{
    Task LoadVentasAsync(IEnumerable<HechoVentaDTO> hechoVentas);
    Task LoadComprasAsync(IEnumerable<HechoCompraDTO> hechoCompras);
}

