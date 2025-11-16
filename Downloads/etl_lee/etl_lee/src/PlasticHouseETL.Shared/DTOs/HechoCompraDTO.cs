namespace PlasticHouseETL.Shared.DTOs;

public class HechoCompraDTO
{
    public int ProveedorPK { get; set; }
    public int ProductoPK { get; set; }
    public int? EmpleadoPK { get; set; }
    public int TiempoPK { get; set; }
    public int? CategoriaPK { get; set; }
    public int Compras_CantidadComprada { get; set; }
    public decimal Compras_PrecioUnitario { get; set; }
    public decimal Compras_SubtotalLinea { get; set; }
    public decimal? Compras_DescuentoLinea { get; set; }
    public decimal Compras_TotalLinea { get; set; }
    public int? Compras_Id_CompraOriginal { get; set; }
    public int? Compras_Id_DetalleOriginal { get; set; }
}

