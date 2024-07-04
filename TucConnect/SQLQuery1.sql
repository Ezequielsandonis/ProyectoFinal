create database MyBlogddb
use MyBlogddb
----------------------------------------------- Roles

create table Roles (
RolId int Identity (1,1) primary key not null,
Nombre varchar(50)
)


insert into Roles values ('Administrador')
insert into Roles values ('Usuario')
DELETE FROM Roles WHERE Nombre = 'Administrador';

create procedure ListarRoles
as begin 
select * from Roles
end

-------------------------------------------------------- Usuarios 

create table Usuarios(
UsuarioId int identity(1,1) not null primary key,
Nombre varchar(50),
Apellido varchar(50),
Correo varchar(100) unique,
Contrasenia varchar(max),
RolId int,
NombreUsuario varchar(50) unique,
Estado bit,
Token varchar(max),
FechaExpiracion datetime 
)


-- Obtener Usuario pór Id
create procedure ObtenerUsuarioPorId
@UsuarioId int
as begin
select * from Usuarios where UsuarioId=@UsuarioId
end


--Registrar usuario
create procedure RegistrarUsuario 
@Nombre varchar(50),
@Apellido varchar(50),
@Correo varchar(100),
@Contrasenia varchar(max),
@RolId int=2,--rol de usuario
@NombreUsuario varchar(50),
@Estado bit=0, --inactiva 
@Token varchar(max),
@FechaExpiracion datetime 
as begin
insert into Usuarios values(@Nombre,@Apellido,@Correo,@Contrasenia,@RolId,@NombreUsuario,@Estado,@Token,@FechaExpiracion) 
end

--activar cuenta 
create procedure ActivarCuenta 
@Token varchar (max),
@Fecha datetime 
as begin 

declare @Correo varchar(100)
declare @FechaExpiracion datetime

set @Correo = (select Correo from Usuarios where Token=@Token)
set @FechaExpiracion= (select FechaExpiracion from Usuarios where Token=@Token)


if @FechaExpiracion < @Fecha 
begin 
update Usuarios set Estado=1 where Token=@Token
update Usuarios set Token = null where Correo=@Correo 
select 1 as Resultado 
end 
else 
begin 
select 0 as Resultado
end 
end

---- Validar usuario
create procedure ValidarUsuario 
@Correo varchar(100)
as begin
select * from Usuarios where Correo=@Correo
end





-- Actualizar token
create procedure ActualizarToken 
@Correo varchar(100),
@Fecha datetime,
@Token varchar(max)
as begin
update Usuarios set Token=@Token, FechaExpiracion=@Fecha where Correo=@Correo
end

-- actualizar el perfil
create procedure ActualizarPerfil
@UsuarioId int, 
@Nombre varchar(50),
@Apellido varchar(50),
@Correo varchar(100)
as begin
update Usuarios set Nombre=@Nombre, Apellido=@Apellido, Correo=@Correo where UsuarioId=@UsuarioId
end

--Actualizar usuario (PAra adminiustrador)
create procedure ActualizarUsuario
@UsuarioId int, 
@Nombre varchar(50),
@Apellido varchar(50),
@RolId int,
@Estado bit
as begin
update Usuarios set Nombre=@Nombre, Apellido=@Apellido, RolId=@RolId, Estado=@Estado where UsuarioId=@UsuarioId
end

--eliminar usuario
CREATE PROCEDURE EliminarUsuario
    @UsuarioId INT
AS
BEGIN
    -- Eliminar posts asociados
    DELETE FROM Post WHERE UsuarioId = @UsuarioId;

    -- Eliminar usuario
    DELETE FROM Usuarios WHERE UsuarioId = @UsuarioId;
END;


--Listar usuarios


create procedure ListarUsuarios
as begin
select * from Usuarios
end

---------------------------------------------------------------------------- Posteos
CREATE TABLE Post (
    PostId INT IDENTITY (1,1) PRIMARY KEY NOT NULL,
    Titulo VARCHAR(500),
    Contenido VARCHAR(MAX),
    Categoria VARCHAR(100),
    FechaCreacion DATETIME,
    Zona VARCHAR(100),
    UsuarioId INT,  
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)  
);

--Eliminar post de uysuarios 

CREATE PROCEDURE EliminarPostsDeUsuario
    @UsuarioId INT
AS
BEGIN
    DELETE FROM Post WHERE UsuarioId = @UsuarioId;
END;


--trigger
CREATE TRIGGER Tr_EliminarPostsDeUsuario
ON Usuarios
FOR DELETE
AS
BEGIN
    DECLARE @UsuarioId INT;
    SELECT @UsuarioId = UsuarioId FROM deleted;
    EXEC EliminarPostsDeUsuario @UsuarioId;
END;


--INSEERTAR
CREATE PROCEDURE InsertarPost
@Titulo VARCHAR(500),
@Contenido VARCHAR(MAX),
@Categoria VARCHAR(100),
@Zona VARCHAR(100),
@FechaCreacion DATETIME,
@UsuarioId INT
AS 
BEGIN
    INSERT INTO Post (Titulo, Contenido, Categoria, Zona, FechaCreacion, UsuarioId)
    VALUES (@Titulo, @Contenido, @Categoria, @Zona, @FechaCreacion, @UsuarioId);
END




-- Actualizar Post
CREATE PROCEDURE ActualizarPost
@PostId INT,
@Titulo VARCHAR(500),
@Contenido VARCHAR(MAX),
@Categoria VARCHAR(100),
@Zona VARCHAR(100),
@UsuarioId INT 
AS 
BEGIN
    UPDATE Post 
    SET Titulo = @Titulo,
        Contenido = @Contenido,
        Categoria = @Categoria,
        Zona = @Zona,
        UsuarioId = @UsuarioId  
    WHERE PostId = @PostId;
END


-- Obtener detalles del post incluyendo UsuarioId
CREATE PROCEDURE ObtenerPostPorId
@PostId INT
AS 
BEGIN
    SELECT P.*, U.UsuarioId AS UsuarioId 
    FROM Post P
    INNER JOIN Usuarios U ON P.UsuarioId = U.UsuarioId 
    WHERE P.PostId = @PostId;
END

-- Obtener el nombre del creador del post
CREATE PROCEDURE ObtenerNombreUsuarioPorPostId
    @PostId INT
AS
BEGIN
    SELECT U.NombreUsuario
    FROM Post P
    INNER JOIN Usuarios U ON P.UsuarioId = U.UsuarioId
    WHERE P.PostId = @PostId;
END

--Obtener Usuario por post para el chat

CREATE PROCEDURE ObtenerUsuarioPorPostId
    @PostId INT
AS
BEGIN
    SELECT U.UsuarioId, U.NombreUsuario  
    FROM Post P
    INNER JOIN Usuarios U ON P.UsuarioId = U.UsuarioId
    WHERE P.PostId = @PostId;
END

--ObtenerPostPorUsuarioId
CREATE PROCEDURE ListarPostPorUsuarioId
    @UsuarioId INT
AS
BEGIN
    SELECT * 
    FROM Post
    WHERE UsuarioId = @UsuarioId;
END


--eliminar post
create procedure ElminarPost
@PostId int
as begin
delete Post where PostId=@PostId
end

-- listar todos los post
create procedure ObtenerTodosLosPost
as begin
select * from Post
end


--filtrar por categorias 
create procedure ObtenerPostPorCategoria
@Categoria varchar(50)
as begin 
select * from Post where Categoria=@Categoria 
end



-- buscar por titulo
create procedure ObtenerPostPorTitulo
@Titulo varchar(500)
as begin 
select * from Post where Titulo like '%'+@Titulo+'%'
end



--buscar por zona

create procedure ObtenerPostPorZona
@Zona varchar(100)
as begin 
select * from Post where Zona=@Zona
end

-------------------------------------------------------------- Comentarios

create table Comentario 
(
ComentarioId  int identity (1,1) primary key not null,
Contenido varchar (max),
FechaCreacion datetime,
UsuarioId int,
PostId int,
ComentarioPadreId int null,
constraint Fk_Comentario_UsuarioId Foreign key  (UsuarioId) references Usuarios(UsuarioId) on delete cascade,
constraint Fk_Comentario_PostId Foreign key (PostId) references Post(PostId) on delete cascade,
constraint Fk_Comentario_ComentariPadreId Foreign key (ComentarioPadreId) references Comentario(ComentarioId) on delete no action
)

create trigger TR_EliminarComentariosHijos on Comentario
after delete 
as begin
delete from Comentario where ComentarioPadreId in (select ComentarioId from deleted)
end

--Comentarios principales
create procedure ObtenerComentariosPorPostId
@PostId int
as begin
select c.ComentarioId, c.Contenido, c.FechaCreacion, c.UsuarioId, c.PostId, u.NombreUsuario from Comentario c
inner join Usuarios u on u.UsuarioId=c.UsuarioId
where c.PostId=@PostId and c.ComentarioPadreId is null
end
--
create procedure ObtenerComentarioHijoPOComentarioId
@ComentarioId int
as begin 
select c.ComentarioId, c.Contenido, c.FechaCreacion, c.UsuarioId, c.PostId, u.NombreUsuario from Comentario c
inner join Usuarios u on u.UsuarioId=c.UsuarioId
where c.ComentarioPadreId=@ComentarioId
end

--agregar comentarios
create procedure AgregarComentario
@Contenido varchar(max),
@FechaCreacion datetime,
@UsuarioId int,
@PostId int,
@ComentarioPadreId int=null
as begin
insert into Comentario values  (@Contenido,@FechaCreacion,@UsuarioId,@PostId,@ComentarioPadreId)
end



