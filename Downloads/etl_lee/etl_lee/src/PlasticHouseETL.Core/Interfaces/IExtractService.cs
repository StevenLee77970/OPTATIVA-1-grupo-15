using PlasticHouseETL.Shared.DTOs;

namespace PlasticHouseETL.Core.Interfaces;

public interface IExtractService
{
    Task<IEnumerable<VentaDTO>> ExtractVentasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<CompraDTO>> ExtractComprasAsync(DateTime fechaInicio, DateTime fechaFin);
}

