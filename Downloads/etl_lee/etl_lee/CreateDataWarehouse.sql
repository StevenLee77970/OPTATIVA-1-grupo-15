-- =============================================
-- Script para crear la Base de Datos Analítica (Data Warehouse)
-- PlasticHouseDW
-- =============================================

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PlasticHouseDW')
BEGIN
    CREATE DATABASE PlasticHouseDW;
END
GO

USE PlasticHouseDW;
GO

-- =============================================
-- TABLAS DE DIMENSIONES
-- =============================================

-- DIM_Tiempo (Time Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Tiempo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Tiempo](
        [TiempoPK] [int] IDENTITY(1,1) NOT NULL,
        [Tiempo_FechaActual] [date] NOT NULL,
        [Tiempo_Anio] [int] NOT NULL,
        [Tiempo_Trimestre] [int] NOT NULL,
        [Tiempo_Mes] [int] NOT NULL,
        [Tiempo_Semana] [int] NOT NULL,
        [Tiempo_DiaDeAnio] [int] NOT NULL,
        [Tiempo_DiaDeMes] [int] NOT NULL,
        [Tiempo_DiaDeSemana] [int] NOT NULL,
        [Tiempo_EsFinSemana] [bit] NOT NULL,
        [Tiempo_EsFeriado] [bit] NOT NULL,
        [ETLLoad] [datetime] NULL,
        CONSTRAINT [PK_DIM_Tiempo] PRIMARY KEY CLUSTERED ([TiempoPK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Tiempo_Fecha] ON [dbo].[DIM_Tiempo] ([Tiempo_FechaActual]);
END
GO

-- DIM_Clientes (Clients Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Clientes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Clientes](
        [ClientePK] [int] IDENTITY(1,1) NOT NULL,
        [Id_Cliente_Original] [int] NOT NULL,
        [Cliente_Id_Empleados] [int] NULL,
        [Cliente_Nombre] [varchar](20) NULL,
        [Cliente_Apellido] [varchar](20) NULL,
        [Cliente_Telefono] [char](15) NULL,
        [Cliente_Direccion] [text] NULL,
        [Cliente_Estado] [bit] NULL,
        [Cliente_Nombre_Completo] [varchar](50) NULL,
        [ETLLoad] [datetime] NULL,
        CONSTRAINT [PK_DIM_Clientes] PRIMARY KEY CLUSTERED ([ClientePK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Clientes_Id_Original] ON [dbo].[DIM_Clientes] ([Id_Cliente_Original]);
END
GO

-- DIM_Empleados (Employees Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Empleados]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Empleados](
        [EmpleadoPK] [int] IDENTITY(1,1) NOT NULL,
        [Id_Empleado_Original] [int] NOT NULL,
        [Empleado_Nombre] [varchar](20) NULL,
        [Empleado_Apellido] [varchar](20) NULL,
        [Empleado_Telefono] [char](15) NULL,
        [Empleado_Numero_Cedula] [char](15) NULL,
        [Empleado_Estado] [bit] NULL,
        [Empleado_Id_Rol] [int] NULL,
        [ETLLoad] [datetime] NULL,
        [ETLExecution] [datetime] NULL,
        CONSTRAINT [PK_DIM_Empleados] PRIMARY KEY CLUSTERED ([EmpleadoPK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Empleados_Id_Original] ON [dbo].[DIM_Empleados] ([Id_Empleado_Original]);
END
GO

-- DIM_Productos (Products Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Productos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Productos](
        [ProductoPK] [int] IDENTITY(1,1) NOT NULL,
        [Id_Productos_Original] [int] NOT NULL,
        [Producto_Nombre_Producto] [nvarchar](38) NULL,
        [Producto_Cantidad_Existente] [nchar](38) NULL,
        [Producto_Precio] [decimal](9, 2) NULL,
        [Producto_Id_Categoria] [int] NULL,
        [Producto_Estado] [bit] NULL,
        [ETLLoad] [datetime] NULL,
        [ETLExecution] [datetime] NULL,
        CONSTRAINT [PK_DIM_Productos] PRIMARY KEY CLUSTERED ([ProductoPK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Productos_Id_Original] ON [dbo].[DIM_Productos] ([Id_Productos_Original]);
END
GO

-- DIM_Categorias (Categories Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Categorias]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Categorias](
        [CategoriaPK] [int] IDENTITY(1,1) NOT NULL,
        [Id_Categoria_Original] [int] NOT NULL,
        [Categoria_Nombre_Categoria] [varchar](50) NULL,
        [Categoria_Descripcion] [varchar](100) NULL,
        [ETLLoad] [datetime] NULL,
        [ETLExecution] [datetime] NULL,
        CONSTRAINT [PK_DIM_Categorias] PRIMARY KEY CLUSTERED ([CategoriaPK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Categorias_Id_Original] ON [dbo].[DIM_Categorias] ([Id_Categoria_Original]);
END
GO

-- DIM_Proveedor (Supplier Dimension)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DIM_Proveedor]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DIM_Proveedor](
        [ProveedorPK] [int] IDENTITY(1,1) NOT NULL,
        [Id_Proveedor_Original] [int] NOT NULL,
        [Nombre_Negocio] [varchar](60) NULL,
        [Nombre] [varchar](20) NULL,
        [Apellido] [varchar](20) NULL,
        [Telefono] [char](15) NULL,
        [Direccion] [text] NULL,
        [Estado] [bit] NULL,
        [ETLLoad] [datetime] NULL,
        [ETLExecution] [datetime] NULL,
        CONSTRAINT [PK_DIM_Proveedor] PRIMARY KEY CLUSTERED ([ProveedorPK] ASC)
    );
    
    CREATE UNIQUE INDEX [IX_DIM_Proveedor_Id_Original] ON [dbo].[DIM_Proveedor] ([Id_Proveedor_Original]);
END
GO

-- =============================================
-- TABLAS DE HECHOS (FACT TABLES)
-- =============================================

-- FACT_Ventas (Sales Fact Table)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FACT_Ventas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FACT_Ventas](
        [FactVentasPK] [int] IDENTITY(1,1) NOT NULL,
        [Ventas_Id_FactVentas] [int] NULL,
        [ClientePK] [int] NULL,
        [ProductoPK] [int] NOT NULL,
        [EmpleadoPK] [int] NULL,
        [TiempoPK] [int] NOT NULL,
        [CategoriaPK] [int] NULL,
        [Ventas_CantidadVendida] [int] NOT NULL,
        [Ventas_PrecioUnitario] [money] NOT NULL,
        [Ventas_SubtotalLinea] [money] NOT NULL,
        [Ventas_DescuentoLinea] [money] NULL,
        [Ventas_TotalLinea] [money] NOT NULL,
        [Ventas_Id_FacturaOriginal] [int] NULL,
        [Ventas_Id_DetalleOriginal] [int] NULL,
        [ETLLoad] [datetime] NULL,
        CONSTRAINT [PK_FACT_Ventas] PRIMARY KEY CLUSTERED ([FactVentasPK] ASC)
    );
    
    -- Foreign Keys
    ALTER TABLE [dbo].[FACT_Ventas] WITH CHECK ADD CONSTRAINT [FK_FACT_Ventas_DIM_Clientes] 
        FOREIGN KEY([ClientePK]) REFERENCES [dbo].[DIM_Clientes] ([ClientePK]);
    
    ALTER TABLE [dbo].[FACT_Ventas] WITH CHECK ADD CONSTRAINT [FK_FACT_Ventas_DIM_Productos] 
        FOREIGN KEY([ProductoPK]) REFERENCES [dbo].[DIM_Productos] ([ProductoPK]);
    
    ALTER TABLE [dbo].[FACT_Ventas] WITH CHECK ADD CONSTRAINT [FK_FACT_Ventas_DIM_Empleados] 
        FOREIGN KEY([EmpleadoPK]) REFERENCES [dbo].[DIM_Empleados] ([EmpleadoPK]);
    
    ALTER TABLE [dbo].[FACT_Ventas] WITH CHECK ADD CONSTRAINT [FK_FACT_Ventas_DIM_Tiempo] 
        FOREIGN KEY([TiempoPK]) REFERENCES [dbo].[DIM_Tiempo] ([TiempoPK]);
    
    ALTER TABLE [dbo].[FACT_Ventas] WITH CHECK ADD CONSTRAINT [FK_FACT_Ventas_DIM_Categorias] 
        FOREIGN KEY([CategoriaPK]) REFERENCES [dbo].[DIM_Categorias] ([CategoriaPK]);
END
GO

-- FACT_Compras (Purchases Fact Table)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FACT_Compras]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FACT_Compras](
        [FactComprasPK] [int] IDENTITY(1,1) NOT NULL,
        [Compras_Id] [int] NULL,
        [ProveedorPK] [int] NOT NULL,
        [ProductoPK] [int] NOT NULL,
        [EmpleadoPK] [int] NULL,
        [TiempoPK] [int] NOT NULL,
        [CategoriaPK] [int] NULL,
        [Compras_CantidadComprada] [int] NOT NULL,
        [Compras_PrecioUnitario] [money] NOT NULL,
        [Compras_SubtotalLinea] [money] NOT NULL,
        [Compras_DescuentoLinea] [money] NULL,
        [Compras_TotalLinea] [money] NOT NULL,
        [Compras_Id_CompraOriginal] [int] NULL,
        [Compras_Id_DetalleOriginal] [int] NULL,
        [ETLLoad] [datetime] NULL,
        CONSTRAINT [PK_FACT_Compras] PRIMARY KEY CLUSTERED ([FactComprasPK] ASC)
    );
    
    -- Foreign Keys
    ALTER TABLE [dbo].[FACT_Compras] WITH CHECK ADD CONSTRAINT [FK_FACT_Compras_DIM_Proveedor] 
        FOREIGN KEY([ProveedorPK]) REFERENCES [dbo].[DIM_Proveedor] ([ProveedorPK]);
    
    ALTER TABLE [dbo].[FACT_Compras] WITH CHECK ADD CONSTRAINT [FK_FACT_Compras_DIM_Productos] 
        FOREIGN KEY([ProductoPK]) REFERENCES [dbo].[DIM_Productos] ([ProductoPK]);
    
    ALTER TABLE [dbo].[FACT_Compras] WITH CHECK ADD CONSTRAINT [FK_FACT_Compras_DIM_Empleados] 
        FOREIGN KEY([EmpleadoPK]) REFERENCES [dbo].[DIM_Empleados] ([EmpleadoPK]);
    
    ALTER TABLE [dbo].[FACT_Compras] WITH CHECK ADD CONSTRAINT [FK_FACT_Compras_DIM_Tiempo] 
        FOREIGN KEY([TiempoPK]) REFERENCES [dbo].[DIM_Tiempo] ([TiempoPK]);
    
    ALTER TABLE [dbo].[FACT_Compras] WITH CHECK ADD CONSTRAINT [FK_FACT_Compras_DIM_Categorias] 
        FOREIGN KEY([CategoriaPK]) REFERENCES [dbo].[DIM_Categorias] ([CategoriaPK]);
END
GO

-- =============================================
-- ÍNDICES PARA MEJORAR EL RENDIMIENTO
-- =============================================

-- Índices para FACT_Ventas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Ventas_TiempoPK' AND object_id = OBJECT_ID('dbo.FACT_Ventas'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Ventas_TiempoPK] ON [dbo].[FACT_Ventas] ([TiempoPK]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Ventas_ProductoPK' AND object_id = OBJECT_ID('dbo.FACT_Ventas'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Ventas_ProductoPK] ON [dbo].[FACT_Ventas] ([ProductoPK]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Ventas_ClientePK' AND object_id = OBJECT_ID('dbo.FACT_Ventas'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Ventas_ClientePK] ON [dbo].[FACT_Ventas] ([ClientePK]);
GO

-- Índices para FACT_Compras
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Compras_TiempoPK' AND object_id = OBJECT_ID('dbo.FACT_Compras'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Compras_TiempoPK] ON [dbo].[FACT_Compras] ([TiempoPK]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Compras_ProductoPK' AND object_id = OBJECT_ID('dbo.FACT_Compras'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Compras_ProductoPK] ON [dbo].[FACT_Compras] ([ProductoPK]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FACT_Compras_ProveedorPK' AND object_id = OBJECT_ID('dbo.FACT_Compras'))
    CREATE NONCLUSTERED INDEX [IX_FACT_Compras_ProveedorPK] ON [dbo].[FACT_Compras] ([ProveedorPK]);
GO

PRINT 'Base de datos analítica (Data Warehouse) creada exitosamente.';
GO

