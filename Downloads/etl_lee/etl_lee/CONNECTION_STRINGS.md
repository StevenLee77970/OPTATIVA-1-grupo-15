# Cadenas de Conexión - Opciones

Si tienes problemas de conexión, prueba estas opciones en `appsettings.json`:

## Opción 1: Instancia SQL Express (Recomendada si tienes SQL Express)
```json
"ConnectionString": "Server=.\\SQLEXPRESS;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
"DataWarehouseDB": "Server=.\\SQLEXPRESS;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
```

## Opción 2: Localhost con instancia
```json
"ConnectionString": "Server=localhost\\SQLEXPRESS;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
"DataWarehouseDB": "Server=localhost\\SQLEXPRESS;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
```

## Opción 3: Nombre de computadora con instancia
```json
"ConnectionString": "Server=JUANLOPEZPC\\SQLEXPRESS;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
"DataWarehouseDB": "Server=JUANLOPEZPC\\SQLEXPRESS;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
```

## Opción 4: Instancia por defecto (si no tienes SQL Express)
```json
"ConnectionString": "Server=.;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
"DataWarehouseDB": "Server=.;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
```

## Opción 5: Localhost sin instancia
```json
"ConnectionString": "Server=localhost;Database=PlasticHouseBD;Encrypt=false;Trusted_Connection=true",
"DataWarehouseDB": "Server=localhost;Database=PlasticHouseDW;Encrypt=false;Trusted_Connection=true"
```

## Notas:
- `.` es equivalente a `localhost` o el nombre de tu computadora
- `\\SQLEXPRESS` es necesario si tienes SQL Server Express instalado
- Si tienes SQL Server Developer o Standard, puede que no necesites especificar la instancia
- Verifica que SQL Server esté ejecutándose: Abre "SQL Server Configuration Manager" y verifica que el servicio esté "Running"

