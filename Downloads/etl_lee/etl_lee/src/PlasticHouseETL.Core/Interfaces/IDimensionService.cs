namespace PlasticHouseETL.Core.Interfaces;

public interface IDimensionService
{
    Task<int> GetOrCreateTiempoIdAsync(DateTime fecha);
    Task<int> GetOrCreateProductoIdAsync(int productoId);
    Task<int> GetOrCreateClienteIdAsync(int clienteId);
    Task<int> GetOrCreateProveedorIdAsync(int proveedorId);
    Task<int?> GetOrCreateEmpleadoIdAsync(int? empleadoId);
    Task<int?> GetOrCreateCategoriaIdAsync(int categoriaId);
}

