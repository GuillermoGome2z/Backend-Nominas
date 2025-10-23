-- Script para insertar datos iniciales en ProyectoNomina2025
USE ProyectoNomina2025;
GO

-- 1. ROLES
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Admin')
BEGIN
    INSERT INTO Roles (Nombre) VALUES ('Admin');
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'RRHH')
BEGIN
    INSERT INTO Roles (Nombre) VALUES ('RRHH');
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Nombre = 'Empleado')
BEGIN
    INSERT INTO Roles (Nombre) VALUES ('Empleado');
END
GO

-- 2. USUARIO ADMIN
DECLARE @AdminRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Admin');

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Correo = 'admin@empresa.com')
BEGIN
    -- Contraseña: Admin123! (hash BCrypt)
    INSERT INTO Usuarios (NombreCompleto, Correo, ClaveHash, Rol, EmpleadoId)
    VALUES ('Administrador General', 'admin@empresa.com', '$2a$11$XuLgqzEqYy3Y3YvJ3wJ3EeR3ZX3Y3YvJ3wJ3EeR3ZX3Y3YvJ3wJ3Ee', 'Admin', NULL);
    
    DECLARE @AdminUsuarioId INT = SCOPE_IDENTITY();
    
    -- Asignar rol Admin al usuario
    INSERT INTO UsuarioRoles (UsuarioId, RolId)
    VALUES (@AdminUsuarioId, @AdminRolId);
END
GO

-- 3. DEPARTAMENTOS
IF NOT EXISTS (SELECT 1 FROM Departamentos)
BEGIN
    INSERT INTO Departamentos (Nombre, Activo) VALUES 
    ('Recursos Humanos', 1),
    ('Tecnología', 1),
    ('Ventas', 1),
    ('Contabilidad', 1),
    ('Operaciones', 1);
END
GO

-- 4. PUESTOS
IF NOT EXISTS (SELECT 1 FROM Puestos)
BEGIN
    DECLARE @DeptoTI INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Tecnología');
    DECLARE @DeptoRRHH INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Recursos Humanos');
    DECLARE @DeptoVentas INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Ventas');
    DECLARE @DeptoContabilidad INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Contabilidad');
    DECLARE @DeptoOperaciones INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Operaciones');

    INSERT INTO Puestos (Nombre, SalarioBase, Activo, DepartamentoId) VALUES 
    -- Tecnología
    ('Desarrollador Senior', 12000.00, 1, @DeptoTI),
    ('Desarrollador Junior', 7000.00, 1, @DeptoTI),
    ('Analista de Sistemas', 9000.00, 1, @DeptoTI),
    -- RRHH
    ('Gerente de RRHH', 10000.00, 1, @DeptoRRHH),
    ('Reclutador', 6000.00, 1, @DeptoRRHH),
    -- Ventas
    ('Ejecutivo de Ventas', 8000.00, 1, @DeptoVentas),
    ('Gerente de Ventas', 11000.00, 1, @DeptoVentas),
    -- Contabilidad
    ('Contador', 8500.00, 1, @DeptoContabilidad),
    -- Operaciones
    ('Supervisor de Operaciones', 9500.00, 1, @DeptoOperaciones);
END
GO

-- 5. EMPLEADOS DE EJEMPLO
IF NOT EXISTS (SELECT 1 FROM Empleados)
BEGIN
    DECLARE @DeptoTI INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Tecnología');
    DECLARE @DeptoRRHH INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Recursos Humanos');
    DECLARE @DeptoVentas INT = (SELECT Id FROM Departamentos WHERE Nombre = 'Ventas');
    
    DECLARE @PuestoDevSenior INT = (SELECT Id FROM Puestos WHERE Nombre = 'Desarrollador Senior');
    DECLARE @PuestoDevJunior INT = (SELECT Id FROM Puestos WHERE Nombre = 'Desarrollador Junior');
    DECLARE @PuestoGerenteRRHH INT = (SELECT Id FROM Puestos WHERE Nombre = 'Gerente de RRHH');
    DECLARE @PuestoEjecutivoVentas INT = (SELECT Id FROM Puestos WHERE Nombre = 'Ejecutivo de Ventas');

    INSERT INTO Empleados (NombreCompleto, DPI, NIT, Correo, Telefono, Direccion, FechaNacimiento, FechaContratacion, EstadoLaboral, SalarioMensual, DepartamentoId, PuestoId) 
    VALUES 
    ('Juan Carlos Pérez', '1234567890101', '12345678', 'juan.perez@empresa.com', '12345678', 'Ciudad de Guatemala, Zona 10', '1990-05-15', '2020-01-10', 'ACTIVO', 12000.00, @DeptoTI, @PuestoDevSenior),
    ('María Fernanda López', '9876543210101', '87654321', 'maria.lopez@empresa.com', '87654321', 'Antigua Guatemala', '1995-08-20', '2022-03-15', 'ACTIVO', 7000.00, @DeptoTI, @PuestoDevJunior),
    ('Carlos Eduardo Ramírez', '1122334455101', '11223344', 'carlos.ramirez@empresa.com', '22334455', 'Quetzaltenango', '1985-03-10', '2018-06-01', 'ACTIVO', 10000.00, @DeptoRRHH, @PuestoGerenteRRHH),
    ('Ana Sofía Martínez', '5566778899101', '55667788', 'ana.martinez@empresa.com', '55667788', 'Escuintla', '1992-11-25', '2021-09-01', 'ACTIVO', 8000.00, @DeptoVentas, @PuestoEjecutivoVentas),
    ('Luis Fernando García', '2233445566101', '22334455', 'luis.garcia@empresa.com', '33445566', 'Mazatenango', '1988-07-18', '2019-04-15', 'ACTIVO', 7000.00, @DeptoTI, @PuestoDevJunior);
END
GO

-- 6. TIPOS DE DOCUMENTO
IF NOT EXISTS (SELECT 1 FROM TiposDocumento)
BEGIN
    INSERT INTO TiposDocumento (Nombre, Descripcion, EsRequerido, Orden) VALUES 
    ('DPI', 'Documento Personal de Identificación', 1, 1),
    ('CV', 'Currículum Vitae', 1, 2),
    ('Título', 'Título académico', 0, 3),
    ('Constancia', 'Constancia de trabajo', 0, 4),
    ('Antecedentes', 'Antecedentes penales y policiacos', 1, 5);
END
GO

PRINT 'Datos iniciales insertados correctamente!';
GO
