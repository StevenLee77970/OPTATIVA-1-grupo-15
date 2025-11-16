namespace PlasticHouseETL.Shared.DTOs;

public class CompraDTO
{
    public int Id_compra { get; set; }
    public int Id_Proveedor { get; set; }
    public DateTime Fecha_Compra { get; set; }
    public decimal Cantidad_Total { get; set; }
    public string? Nombre_Negocio { get; set; }
    public int Id_Detalle_Compra { get; set; }
    public int Id_Productos { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio_Unitario { get; set; }
    public decimal Linea_Total { get; set; }
    // Campos adicionales de las tablas relacionadas
    public string? Proveedor_Nombre_Negocio { get; set; }
    public string? Proveedor_Nombre { get; set; }
    public string? Proveedor_Apellido { get; set; }
    public string? Proveedor_Telefono { get; set; }
    public string? Proveedor_Direccion { get; set; }
    public bool? Proveedor_Estado { get; set; }
    public string? Producto_Nombre_Producto { get; set; }
    public int? Producto_Categoria_Id { get; set; }
    public string? Categoria_Nombre { get; set; }
    public string? Categoria_Descripcion { get; set; }
}

