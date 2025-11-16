namespace PlasticHouseETL.Shared.DTOs;

public class HechoVentaDTO
{
    public int ClientePK { get; set; }
    public int ProductoPK { get; set; }
    public int? EmpleadoPK { get; set; }
    public int TiempoPK { get; set; }
    public int? CategoriaPK { get; set; }
    public int Ventas_CantidadVendida { get; set; }
    public decimal Ventas_PrecioUnitario { get; set; }
    public decimal Ventas_SubtotalLinea { get; set; }
    public decimal? Ventas_DescuentoLinea { get; set; }
    public decimal Ventas_TotalLinea { get; set; }
    public int? Ventas_Id_FacturaOriginal { get; set; }
    public int? Ventas_Id_DetalleOriginal { get; set; }
}

