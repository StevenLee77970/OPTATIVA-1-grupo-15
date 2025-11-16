namespace PlasticHouseETL.Shared.DTOs;

public class VentaDTO
{
    public int Id_Venta { get; set; }
    public int Id_Cliente { get; set; }
    public DateTime Fecha_Venta { get; set; }
    public decimal Cantidad_Total { get; set; }
    public int Id_Detalle_Venta { get; set; }
    public int Id_Productos { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio_Unitario { get; set; }
    public decimal Linea_Total { get; set; }
    // Campos adicionales de las tablas relacionadas
    public string? Cliente_Nombre { get; set; }
    public string? Cliente_Apellido { get; set; }
    public string? Cliente_Telefono { get; set; }
    public string? Cliente_Direccion { get; set; }
    public bool? Cliente_Estado { get; set; }
    public int? Cliente_Id_Empleados { get; set; }
    public string? Producto_Nombre_Producto { get; set; }
    public int? Producto_Categoria_Id { get; set; }
    public string? Categoria_Nombre { get; set; }
    public string? Categoria_Descripcion { get; set; }
    public int? Empleado_Id { get; set; }
    public string? Empleado_Nombre { get; set; }
    public string? Empleado_Apellido { get; set; }
}

