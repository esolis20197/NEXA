CREATE DATABASE Nexa;
GO

USE Nexa;
GO

-- ADMINISTRADOR
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario NVARCHAR(100) NOT NULL,
    NombreCompleto NVARCHAR(150) NOT NULL,
    Correo NVARCHAR(150) NOT NULL,
    Cedula NVARCHAR(50) NOT NULL,
    Telefono NVARCHAR(50),
    Contraseña NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL --'Administrador', 'Empleado' o 'Usuario'
);


-- INVENTARIO
CREATE TABLE Inventario (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255),
    Precio DECIMAL(10, 2) NOT NULL,
    Cantidad INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL  -- 'Producto', 'Material'
);


-- MODULO CARRITO DE COMPRAS

CREATE TABLE CarritoCompras (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    Producto NVARCHAR(255) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL CHECK (Precio >= 0),
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Total AS (Precio * Cantidad) PERSISTED,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente' -- 'Pendiente', 'Procesado', 'Cancelado'
);


-- MODULO EMPLEADOS

CREATE TABLE Empleados (
    IdEmpleado INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Cargo NVARCHAR(100) NOT NULL,
    Departamento NVARCHAR(100) NOT NULL,
    Salario DECIMAL(12,2) NOT NULL,
    Correo NVARCHAR(150) UNIQUE NOT NULL,
    Telefono NVARCHAR(50),
    Estado NVARCHAR(50) DEFAULT 'Activo' -- Por ejemplo: Activo, Inactivo
);

CREATE TABLE Roles (
    IdRol INT PRIMARY KEY IDENTITY(1,1),
    NombreRol NVARCHAR(50) UNIQUE NOT NULL -- Ejemplo: 'Administrador', 'Empleado', 'Usuario'
);

CREATE TABLE EmpleadosRoles (
    IdEmpleado INT,
    IdRol INT,
    PRIMARY KEY (IdEmpleado, IdRol),
    FOREIGN KEY (IdEmpleado) REFERENCES Empleados(IdEmpleado),
    FOREIGN KEY (IdRol) REFERENCES Roles(IdRol)
);

-- MODULO PROYECTOS
CREATE TABLE Proyectos (
    IdProyecto INT PRIMARY KEY IDENTITY(1,1),
    NombreProyecto NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(MAX),
    FechaInicio DATE,
    FechaFin DATE,
    Estado NVARCHAR(50) DEFAULT 'En Progreso', -- En Progreso, Finalizado, Cancelado
    ClienteId INT, -- Si el proyecto está asociado a un cliente
    FOREIGN KEY (ClienteId) REFERENCES Usuarios(Id)
);

CREATE TABLE ProyectosEmpleados (
    IdProyecto INT,
    IdEmpleado INT,
    FechaAsignacion DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (IdProyecto, IdEmpleado),
    FOREIGN KEY (IdProyecto) REFERENCES Proyectos(IdProyecto),
    FOREIGN KEY (IdEmpleado) REFERENCES Empleados(IdEmpleado)
);
 
CREATE TABLE Asignaciones_Proyecto (
    id_asignacion INT IDENTITY(1,1) PRIMARY KEY,
    id_proyecto INT NOT NULL,
    id_empleado INT NOT NULL,
    fecha_asignacion DATE DEFAULT GETDATE(),
    FOREIGN KEY (id_proyecto) REFERENCES Proyectos(IdProyecto),
    FOREIGN KEY (id_empleado) REFERENCES Empleados(IdEmpleado)
);

CREATE TABLE Fotos_Proyecto (
    id_foto INT IDENTITY(1,1) PRIMARY KEY,
    id_proyecto INT NOT NULL,
    url_foto VARCHAR(500) NOT NULL,
    descripcion VARCHAR(255),
    FOREIGN KEY (id_proyecto) REFERENCES Proyectos(IdProyecto)
);


CREATE TABLE Etapas_Proyecto (
    id_etapa INT IDENTITY(1,1) PRIMARY KEY,
    id_proyecto INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    estado VARCHAR(50) NOT NULL,
    comentario VARCHAR(500),
    orden INT,
    FOREIGN KEY (id_proyecto) REFERENCES Proyectos(IdProyecto)
);

CREATE TABLE Notas_Proyecto (
    id_nota INT IDENTITY(1,1) PRIMARY KEY,
    id_proyecto INT NOT NULL,
    id_usuario INT NOT NULL,
    contenido TEXT NOT NULL,
    fecha DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (id_proyecto) REFERENCES Proyectos(IdProyecto),
    FOREIGN KEY (id_usuario) REFERENCES Empleados(IdEmpleado)
);

CREATE TABLE Archivos_Adjuntos (
    id_archivo INT IDENTITY(1,1) PRIMARY KEY,
    id_nota INT NOT NULL,
    nombre_archivo VARCHAR(255),
    ruta_archivo VARCHAR(500),
    FOREIGN KEY (id_nota) REFERENCES Notas_Proyecto(id_nota)
);




-- MODULO REPORTES

CREATE TABLE historial_reportes (
    id_historial INT PRIMARY KEY IDENTITY(1,1),
    tipo_reporte VARCHAR(100),
    filtros_aplicados VARCHAR(500),
    fecha_generacion DATE,
    nombre_archivo VARCHAR(200)
);

-- MODULO MARKETING

CREATE TABLE GruposClientes (
    IdGrupo INT PRIMARY KEY IDENTITY(1,1),
    NombreGrupo NVARCHAR(150) NOT NULL
);

CREATE TABLE ClientesGrupos (
    IdGrupo INT NOT NULL,
    IdCliente INT NOT NULL,
    PRIMARY KEY (IdGrupo, IdCliente),
    FOREIGN KEY (IdGrupo) REFERENCES GruposClientes(IdGrupo),
    FOREIGN KEY (IdCliente) REFERENCES Usuarios(Id)
);

CREATE TABLE CorreosPromocionales (
    IdCorreo INT PRIMARY KEY IDENTITY(1,1),
    Asunto NVARCHAR(200) NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,
    FechaEnvio DATETIME NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Programado', -- 'Programado', 'Enviado'
    IdGrupo INT NOT NULL,
    FOREIGN KEY (IdGrupo) REFERENCES GruposClientes(IdGrupo)
);


-- MODULO CONTABILIDAD

CREATE TABLE Gastos (
    IdGasto INT PRIMARY KEY IDENTITY(1,1),
    NombreGasto NVARCHAR(150) NOT NULL,
    Fecha DATE NOT NULL,
    Descripcion NVARCHAR(255),
    Monto DECIMAL(10,2) NOT NULL,
    Proveedor NVARCHAR(150),
    RegistradoPor INT NOT NULL,
    FOREIGN KEY (RegistradoPor) REFERENCES Usuarios(Id)
);

CREATE TABLE CuentasPorPagar (
    IdCuentaPagar INT PRIMARY KEY IDENTITY(1,1),
    NombreDeuda NVARCHAR(150) NOT NULL,
    Fecha DATE NOT NULL,
    Descripcion NVARCHAR(255),
    Monto DECIMAL(10,2) NOT NULL,
    Proveedor NVARCHAR(150),
    PlazaParaPagar DATE NOT NULL,
    RegistradoPor INT NOT NULL,
    FOREIGN KEY (RegistradoPor) REFERENCES Usuarios(Id)
);

CREATE TABLE CierresFinancieros (
    IdCierre INT PRIMARY KEY IDENTITY(1,1),
    Tipo NVARCHAR(20) NOT NULL CHECK (Tipo IN ('Diario', 'Semanal', 'Mensual')),
    FechaGeneracion DATETIME DEFAULT GETDATE(),
    Balance DECIMAL(12,2) NOT NULL,
    TotalGastos DECIMAL(12,2) NOT NULL,
    TotalIngresos DECIMAL(12,2) NOT NULL,
    TotalCuentasPorPagar DECIMAL(12,2) NOT NULL
);

CREATE TABLE NotificacionesContables (
    IdNotContables INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,
    Tipo NVARCHAR(100), -- Se define el tipo de notificación, Cierre Financiero, Alerta de pago, etc...
    Mensaje NVARCHAR(MAX),
    EnviadoEn DATETIME DEFAULT GETDATE(),
    Leido BIT DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
);

-- MODULO PORTAL DE CLIENTES

CREATE TABLE Pedidos (
    IdPedido INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT,
    FechaPedido DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2),
    FOREIGN KEY (IdCliente) REFERENCES Usuarios(Id)
);

CREATE TABLE DetallePedido (
    IdDetallePedido INT PRIMARY KEY IDENTITY(1,1),
    IdPedido INT,
    NombreProducto NVARCHAR(150),
    Cantidad INT,
    PrecioUnitario DECIMAL(10,2),
    PrecioTotal AS (Cantidad * PrecioUnitario) PERSISTED,
    FOREIGN KEY (IdPedido) REFERENCES Pedidos(IdPedido)
);

CREATE TABLE Facturas (
    IdFactura INT PRIMARY KEY IDENTITY(1,1),
    IdPedido INT,
    Fecha DATE,
    Total DECIMAL(10,2),
    FOREIGN KEY (IdPedido) REFERENCES Pedidos(IdPedido)
);

CREATE TABLE DetalleFactura (
    IdDetalleFactura INT PRIMARY KEY IDENTITY(1,1),
    IdFactura INT,
    NombreProducto NVARCHAR(150),
    Cantidad INT,
    PrecioUnitario DECIMAL(10,2),
    PrecioTotal AS (Cantidad * PrecioUnitario) PERSISTED,
    FOREIGN KEY (IdFactura) REFERENCES Facturas(IdFactura)
);

CREATE TABLE ChatSoporte (
    IdChat INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT,
    IdAgente INT,
    FechaInicio DATETIME DEFAULT GETDATE(),
    FechaFin DATETIME,
    Estado NVARCHAR(50), -- EnCurso, Finalizado
    FOREIGN KEY (IdCliente) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdAgente) REFERENCES Usuarios(Id)
);

CREATE TABLE MensajesChat (
    IdMensaje INT PRIMARY KEY IDENTITY(1,1),
    IdChat INT,
    Emisor NVARCHAR(50), -- Cliente o Agente
    Mensaje NVARCHAR(MAX),
    FechaEnvio DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdChat) REFERENCES ChatSoporte(IdChat)
);

CREATE TABLE SolicitudVisita (
    IdVisita INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT,
    FechaSolicitud DATETIME DEFAULT GETDATE(),
    Estado NVARCHAR(50), -- Pendiente, Aprobado, Rechazado
    Comentario NVARCHAR(MAX),
    FOREIGN KEY (IdCliente) REFERENCES Usuarios(Id)
);

CREATE TABLE AsignacionTecnico (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdSolicitud INT,
    IdTecnico INT,
    FechaAsignacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdSolicitud) REFERENCES SolicitudVisita(IdVisita),
    FOREIGN KEY (IdTecnico) REFERENCES Usuarios(Id)
);

-- MODULO PERMISOS DE INSTALACION

CREATE TABLE PermisosInstalacion (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdProyecto INT NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- 'Pendiente', 'Aprobado', 'Rechazado'
    Comentario NVARCHAR(MAX),
    RevisadoPor INT NULL,
    FechaRevision DATETIME NULL,
    FOREIGN KEY (IdProyecto) REFERENCES Proyectos(IdProyecto),
    FOREIGN KEY (RevisadoPor) REFERENCES Usuarios(Id)
);

CREATE TABLE DocumentosPermisosInstalacion ( -- Definir si se van a almacenar en la nube o en la base de datos
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdPermiso INT NOT NULL,
    NombreArchivo NVARCHAR(255) NOT NULL,         
    TipoArchivo NVARCHAR(10) NOT NULL,            
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Subido',
    UrlArchivo NVARCHAR(500),                     
    FechaDeSubida DATETIME DEFAULT GETDATE(),          
    FOREIGN KEY (IdPermiso) REFERENCES PermisosInstalacion(Id)
);
