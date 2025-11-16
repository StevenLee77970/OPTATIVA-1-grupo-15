using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlasticHouseETL.Core.Interfaces;

namespace PlasticHouseETL.Infrastructure.Services;

public class DimensionService : IDimensionService
{
    private readonly string _dwConnectionString;
    private readonly string _operationalConnectionString;
    private readonly ILogger<DimensionService> _logger;

    public DimensionService(IConfiguration configuration, ILogger<DimensionService> logger)
    {
        _dwConnectionString = configuration.GetConnectionString("DataWarehouseDB") 
            ?? throw new ArgumentNullException(nameof(configuration), "DataWarehouseDB connection string is required");
        _operationalConnectionString = configuration.GetConnectionString("ConnectionString") 
            ?? throw new ArgumentNullException(nameof(configuration), "ConnectionString is required");
        _logger = logger;
    }

    public async Task<int> GetOrCreateTiempoIdAsync(DateTime fecha)
    {
        using var connection = new SqlConnection(_dwConnectionString);
        
        // Buscar si existe
        var query = "SELECT TiempoPK FROM DIM_Tiempo WHERE Tiempo_FechaActual = @Fecha";
        var tiempoId = await connection.QueryFirstOrDefaultAsync<int?>(query, new { Fecha = fecha.Date });
        
        if (tiempoId.HasValue)
        {
            return tiempoId.Value;
        }
        
        // Crear nuevo registro de tiempo
        var insertQuery = @"
            INSERT INTO DIM_Tiempo (
                Tiempo_FechaActual, Tiempo_Anio, Tiempo_Trimestre, Tiempo_Mes, Tiempo_Semana,
                Tiempo_DiaDeAnio, Tiempo_DiaDeMes, Tiempo_DiaDeSemana, Tiempo_EsFinSemana, Tiempo_EsFeriado, ETLLoad
            )
            VALUES (
                @Fecha, @Anio, @Trimestre, @Mes, @Semana, @DiaDeAnio, @DiaDeMes, @DiaDeSemana, 
                @EsFinSemana, @EsFeriado, GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var newTiempoId = await connection.QuerySingleAsync<int>(insertQuery, new
        {
            Fecha = fecha.Date,
            Anio = fecha.Year,
            Trimestre = (fecha.Month - 1) / 3 + 1,
            Mes = fecha.Month,
            Semana = GetWeekOfYear(fecha),
            DiaDeAnio = fecha.DayOfYear,
            DiaDeMes = fecha.Day,
            DiaDeSemana = (int)fecha.DayOfWeek == 0 ? 7 : (int)fecha.DayOfWeek, // Domingo = 7
            EsFinSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday,
            EsFeriado = false
        });
        
        _logger.LogInformation("Created new DIM_Tiempo record for {Fecha} with Id {Id}", fecha, newTiempoId);
        return newTiempoId;
    }

    public async Task<int> GetOrCreateProductoIdAsync(int productoId)
    {
        using var dwConnection = new SqlConnection(_dwConnectionString);
        
        // Buscar por Id_Productos_Original en DIM_Productos
        var query = "SELECT ProductoPK FROM DIM_Productos WHERE Id_Productos_Original = @ProductoId";
        var productoPK = await dwConnection.QueryFirstOrDefaultAsync<int?>(query, new { ProductoId = productoId });
        
        if (productoPK.HasValue)
        {
            return productoPK.Value;
        }
        
        // Si no existe, obtener información del producto desde la BD operacional
        using var opConnection = new SqlConnection(_operationalConnectionString);
        
        var productoQuery = @"
            SELECT 
                p.Id_Productos,
                p.Nombre_Producto,
                p.Cantidad_Existente,
                p.Precio,
                p.categoria_Id,
                p.Estado
            FROM Productos p
            WHERE p.Id_Productos = @ProductoId";
        
        var productoData = await opConnection.QueryFirstOrDefaultAsync<dynamic>(productoQuery, new { ProductoId = productoId });
        
        if (productoData == null)
        {
            _logger.LogWarning("Producto with Id {ProductoId} not found in operational DB", productoId);
            return 0;
        }
        
        // Obtener o crear categoría
        var categoriaPK = await GetOrCreateCategoriaIdAsync(productoData.categoria_Id);
        
        // Crear DimProducto
        var insertQuery = @"
            INSERT INTO DIM_Productos (
                Id_Productos_Original, Producto_Nombre_Producto, Producto_Cantidad_Existente, Producto_Precio,
                Producto_Id_Categoria, Producto_Estado, ETLLoad
            )
            VALUES (
                @IdProductosOriginal, @NombreProducto, @CantidadExistente, @Precio, @CategoriaId, @Estado, GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var newProductoPK = await dwConnection.QuerySingleAsync<int>(insertQuery, new
        {
            IdProductosOriginal = productoData.Id_Productos,
            NombreProducto = productoData.Nombre_Producto,
            CantidadExistente = productoData.Cantidad_Existente,
            Precio = productoData.Precio,
            CategoriaId = productoData.categoria_Id,
            Estado = productoData.Estado
        });
        
        _logger.LogInformation("Created new DIM_Productos record with Id {Id}", newProductoPK);
        return newProductoPK;
    }

    public async Task<int> GetOrCreateClienteIdAsync(int clienteId)
    {
        using var dwConnection = new SqlConnection(_dwConnectionString);
        
        // Buscar en DIM_Clientes por Id_Cliente_Original
        var query = "SELECT ClientePK FROM DIM_Clientes WHERE Id_Cliente_Original = @ClienteId";
        var clientePK = await dwConnection.QueryFirstOrDefaultAsync<int?>(query, new { ClienteId = clienteId });
        
        if (clientePK.HasValue)
        {
            return clientePK.Value;
        }
        
        // Obtener información del cliente desde la BD operacional
        using var opConnection = new SqlConnection(_operationalConnectionString);
        var clienteQuery = @"
            SELECT 
                Id_Cliente, Id_Empleados, Nombre, Apellido, Telefono, Direccion, Estado
            FROM Clientes
            WHERE Id_Cliente = @ClienteId";
        var clienteData = await opConnection.QueryFirstOrDefaultAsync<dynamic>(clienteQuery, new { ClienteId = clienteId });
        
        if (clienteData == null)
        {
            _logger.LogWarning("Cliente with Id {ClienteId} not found in operational DB", clienteId);
            return 0;
        }
        
        // Crear nuevo cliente en DIM_Clientes
        var insertQuery = @"
            INSERT INTO DIM_Clientes (
                Id_Cliente_Original, Cliente_Id_Empleados, Cliente_Nombre, Cliente_Apellido, Cliente_Telefono,
                Cliente_Direccion, Cliente_Estado, Cliente_Nombre_Completo, ETLLoad
            )
            VALUES (
                @IdClienteOriginal, @IdEmpleados, @Nombre, @Apellido, @Telefono, @Direccion, @Estado, 
                @NombreCompleto, GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var nombreCompleto = $"{clienteData.Nombre} {clienteData.Apellido}";
        var newClientePK = await dwConnection.QuerySingleAsync<int>(insertQuery, new
        {
            IdClienteOriginal = clienteData.Id_Cliente,
            IdEmpleados = clienteData.Id_Empleados,
            Nombre = clienteData.Nombre,
            Apellido = clienteData.Apellido,
            Telefono = clienteData.Telefono,
            Direccion = clienteData.Direccion,
            Estado = clienteData.Estado,
            NombreCompleto = nombreCompleto
        });
        
        _logger.LogInformation("Created new DIM_Clientes record with Id {Id}", newClientePK);
        return newClientePK;
    }

    public async Task<int> GetOrCreateProveedorIdAsync(int proveedorId)
    {
        using var dwConnection = new SqlConnection(_dwConnectionString);
        
        // Buscar en DIM_Proveedor por Id_Proveedor_Original
        var query = "SELECT ProveedorPK FROM DIM_Proveedor WHERE Id_Proveedor_Original = @ProveedorId";
        var proveedorPK = await dwConnection.QueryFirstOrDefaultAsync<int?>(query, new { ProveedorId = proveedorId });
        
        if (proveedorPK.HasValue)
        {
            return proveedorPK.Value;
        }
        
        // Obtener información del proveedor desde la BD operacional
        using var opConnection = new SqlConnection(_operationalConnectionString);
        var proveedorQuery = @"
            SELECT 
                Id_Proveedor, Nombre_Negocio, Nombre, Apellido, Telefono, Direccion, Estado
            FROM Proveedor
            WHERE Id_Proveedor = @ProveedorId";
        var proveedorData = await opConnection.QueryFirstOrDefaultAsync<dynamic>(proveedorQuery, new { ProveedorId = proveedorId });
        
        if (proveedorData == null)
        {
            _logger.LogWarning("Proveedor with Id {ProveedorId} not found in operational DB", proveedorId);
            return 0;
        }
        
        // Crear nuevo proveedor en DIM_Proveedor
        var insertQuery = @"
            INSERT INTO DIM_Proveedor (
                Id_Proveedor_Original, Nombre_Negocio, Nombre, Apellido, Telefono, Direccion, Estado, ETLLoad, ETLExecution
            )
            VALUES (
                @IdProveedorOriginal, @NombreNegocio, @Nombre, @Apellido, @Telefono, @Direccion, @Estado, GETDATE(), GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var newProveedorPK = await dwConnection.QuerySingleAsync<int>(insertQuery, new
        {
            IdProveedorOriginal = proveedorData.Id_Proveedor,
            NombreNegocio = proveedorData.Nombre_Negocio,
            Nombre = proveedorData.Nombre,
            Apellido = proveedorData.Apellido,
            Telefono = proveedorData.Telefono,
            Direccion = proveedorData.Direccion,
            Estado = proveedorData.Estado
        });
        
        _logger.LogInformation("Created new DIM_Proveedor record with Id {Id}", newProveedorPK);
        return newProveedorPK;
    }

    public async Task<int?> GetOrCreateEmpleadoIdAsync(int? empleadoId)
    {
        if (!empleadoId.HasValue)
            return null;

        using var dwConnection = new SqlConnection(_dwConnectionString);
        
        // Buscar en DIM_Empleados por Id_Empleado_Original
        var query = "SELECT EmpleadoPK FROM DIM_Empleados WHERE Id_Empleado_Original = @EmpleadoId";
        var empleadoPK = await dwConnection.QueryFirstOrDefaultAsync<int?>(query, new { EmpleadoId = empleadoId });
        
        if (empleadoPK.HasValue)
        {
            return empleadoPK.Value;
        }
        
        // Obtener información del empleado desde la BD operacional
        using var opConnection = new SqlConnection(_operationalConnectionString);
        var empleadoQuery = @"
            SELECT 
                Id_Empleados, Nombre, Apellido, Telefono, Numero_Cedula, Estado, Id_Rol
            FROM Empleados
            WHERE Id_Empleados = @EmpleadoId";
        var empleadoData = await opConnection.QueryFirstOrDefaultAsync<dynamic>(empleadoQuery, new { EmpleadoId = empleadoId });
        
        if (empleadoData == null)
        {
            _logger.LogWarning("Empleado with Id {EmpleadoId} not found in operational DB", empleadoId);
            return null;
        }
        
        // Crear nuevo empleado en DIM_Empleados
        var insertQuery = @"
            INSERT INTO DIM_Empleados (
                Id_Empleado_Original, Empleado_Nombre, Empleado_Apellido, Empleado_Telefono, Empleado_Numero_Cedula,
                Empleado_Estado, Empleado_Id_Rol, ETLLoad, ETLExecution
            )
            VALUES (
                @IdEmpleadoOriginal, @Nombre, @Apellido, @Telefono, @NumeroCedula, @Estado, @IdRol, GETDATE(), GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var newEmpleadoPK = await dwConnection.QuerySingleAsync<int>(insertQuery, new
        {
            IdEmpleadoOriginal = empleadoData.Id_Empleados,
            Nombre = empleadoData.Nombre,
            Apellido = empleadoData.Apellido,
            Telefono = empleadoData.Telefono,
            NumeroCedula = empleadoData.Numero_Cedula,
            Estado = empleadoData.Estado,
            IdRol = empleadoData.Id_Rol
        });
        
        _logger.LogInformation("Created new DIM_Empleados record with Id {Id}", newEmpleadoPK);
        return newEmpleadoPK;
    }

    public async Task<int?> GetOrCreateCategoriaIdAsync(int categoriaId)
    {
        using var dwConnection = new SqlConnection(_dwConnectionString);
        
        // Buscar en DIM_Categorias por Id_Categoria_Original
        var query = "SELECT CategoriaPK FROM DIM_Categorias WHERE Id_Categoria_Original = @CategoriaId";
        var categoriaPK = await dwConnection.QueryFirstOrDefaultAsync<int?>(query, new { CategoriaId = categoriaId });
        
        if (categoriaPK.HasValue)
        {
            return categoriaPK.Value;
        }
        
        // Obtener información de la categoría desde la BD operacional
        using var opConnection = new SqlConnection(_operationalConnectionString);
        var categoriaQuery = @"
            SELECT 
                Id_Categoria, Nombre_Categoria, Descripcion
            FROM Categorias
            WHERE Id_Categoria = @CategoriaId";
        var categoriaData = await opConnection.QueryFirstOrDefaultAsync<dynamic>(categoriaQuery, new { CategoriaId = categoriaId });
        
        if (categoriaData == null)
        {
            _logger.LogWarning("Categoria with Id {CategoriaId} not found in operational DB", categoriaId);
            return null;
        }
        
        // Crear nueva categoría en DIM_Categorias
        var insertQuery = @"
            INSERT INTO DIM_Categorias (
                Id_Categoria_Original, Categoria_Nombre_Categoria, Categoria_Descripcion, ETLLoad, ETLExecution
            )
            VALUES (
                @IdCategoriaOriginal, @NombreCategoria, @Descripcion, GETDATE(), GETDATE()
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        var newCategoriaPK = await dwConnection.QuerySingleAsync<int>(insertQuery, new
        {
            IdCategoriaOriginal = categoriaData.Id_Categoria,
            NombreCategoria = categoriaData.Nombre_Categoria,
            Descripcion = categoriaData.Descripcion
        });
        
        _logger.LogInformation("Created new DIM_Categorias record with Id {Id}", newCategoriaPK);
        return newCategoriaPK;
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}

