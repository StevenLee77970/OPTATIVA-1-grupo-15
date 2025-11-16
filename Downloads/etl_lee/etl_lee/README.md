# 🚀 Proyecto ETL - PlasticHouse

Proyecto ETL (Extract, Transform, Load) para sincronizar datos desde la base de datos operacional (`PlasticHouseBD`) hacia el Data Warehouse (`PlasticHouseDW`).

## 📋 Estructura del Proyecto

```
PlasticHouseETL/
├── src/
│   ├── PlasticHouseETL.Console/          # Aplicación de consola principal
│   ├── PlasticHouseETL.Core/              # Interfaces y contratos
│   ├── PlasticHouseETL.Infrastructure/     # Implementaciones de servicios
│   └── PlasticHouseETL.Shared/           # DTOs compartidos
├── CreateDataWarehouse.sql                # Script para crear el Data Warehouse
└── PlasticHouseETL.sln                    # Solución .NET
```

## 🏗️ Arquitectura

El proyecto sigue el patrón de arquitectura limpia:

- **Shared**: DTOs (Data Transfer Objects) compartidos entre capas
- **Core**: Interfaces que definen los contratos de los servicios ETL
- **Infrastructure**: Implementaciones de los servicios ETL
- **Console**: Aplicación de consola que orquesta el proceso ETL

## 🔄 Proceso ETL

### Extract (Extracción)
- Extrae datos de las tablas operacionales:
  - `Ventas` y `Detalle_Venta`
  - `Compra` y `Detalle_Compra`

### Transform (Transformación)
- Transforma los datos al formato Star Schema:
  - Crea o obtiene IDs de dimensiones (Tiempo, Producto, Cliente, Empleado, Proveedor, Categoría)
  - Calcula métricas (cantidades, precios, totales)
  - Mapea datos al formato de las tablas de hechos

### Load (Carga)
- Carga los datos transformados al Data Warehouse:
  - `FACT_Ventas`
  - `FACT_Compras`
  - Usa MERGE statements para insertar o actualizar

## 📊 Esquema del Data Warehouse

### Tablas de Dimensiones
- `DIM_Tiempo`: Dimensiones temporales (año, mes, trimestre, semana, etc.)
- `DIM_Clientes`: Información de clientes
- `DIM_Empleados`: Información de empleados
- `DIM_Productos`: Información de productos
- `DIM_Categorias`: Categorías de productos
- `DIM_Proveedor`: Información de proveedores

### Tablas de Hechos
- `FACT_Ventas`: Hechos de ventas con medidas y claves foráneas a dimensiones
- `FACT_Compras`: Hechos de compras con medidas y claves foráneas a dimensiones

## ⚙️ Configuración

### 1. Crear el Data Warehouse

Ejecuta el script SQL para crear la base de datos analítica:

```sql
-- Ejecutar CreateDataWarehouse.sql en SQL Server Management Studio
```

### 2. Configurar appsettings.json

Edita `src/PlasticHouseETL.Console/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=LAPTOPDENAVIESK;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
    "DataWarehouseDB": "Server=LAPTOPDENAVIESK;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
  },
  "ETLSettings": {
    "BatchSize": 1000,
    "MaxRetries": 3,
    "RetryDelaySeconds": 5,
    "ExecutionIntervalHours": 6,
    "LastExecutionDate": null
  }
}
```

## 🚀 Uso

### Compilar el proyecto

```bash
dotnet build
```

### Ejecutar ETL Incremental (últimas 24 horas)

```bash
dotnet run --project src/PlasticHouseETL.Console/PlasticHouseETL.Console.csproj
```

o

```bash
dotnet run --project src/PlasticHouseETL.Console/PlasticHouseETL.Console.csproj incremental
```

### Ejecutar ETL Completo (desde 2020)

```bash
dotnet run --project src/PlasticHouseETL.Console/PlasticHouseETL.Console.csproj full
```

## 📊 Logs

Los logs se guardan en:
- Consola: Salida estándar
- Archivo: `logs/etl-YYYYMMDD.txt` (rotación diaria)

## 🔧 Tecnologías Utilizadas

- **.NET 8.0**
- **Dapper**: ORM ligero para acceso a datos
- **Microsoft.Data.SqlClient**: Cliente SQL Server
- **Serilog**: Framework de logging
- **Microsoft.Extensions**: Dependency Injection, Configuration, Logging

## 📝 Notas Importantes

1. **Primera Ejecución**: Ejecuta `full` la primera vez para poblar todo el Data Warehouse.

2. **Ejecuciones Periódicas**: Usa `incremental` para ejecuciones programadas (cada 6 horas por defecto).

3. **Programación**: Puedes usar Windows Task Scheduler para ejecutar periódicamente:
   ```powershell
   # Ejemplo: Ejecutar cada 6 horas
   schtasks /create /tn "ETL PlasticHouse" /tr "dotnet run --project src\PlasticHouseETL.Console\PlasticHouseETL.Console.csproj" /sc hourly /mo 6
   ```

4. **Dimensiones**: El servicio `DimensionService` crea automáticamente las dimensiones faltantes en el Data Warehouse basándose en los datos de la base operacional.

## 🐛 Troubleshooting

- **Error de conexión**: Verifica las cadenas de conexión en `appsettings.json`
- **Error de permisos**: Asegúrate de que el usuario tenga permisos de lectura en la BD operacional y lectura/escritura en el Data Warehouse
- **Error de tablas no encontradas**: Asegúrate de haber ejecutado el script `CreateDataWarehouse.sql` primero

