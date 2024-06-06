using System.Data.SqlClient;
using System.Data;
using TucConnect.Models;

namespace TucConnect.Data.Servicios
{
    public class UsuarioServicio
    {
        private readonly Contexto _contexto;
        public UsuarioServicio(Contexto con)
        {
            _contexto = con;
        }

        //metodo para actualizar token ya existente para activar cuenta
        public void ActualizarToken(string correo)
        {
            using (SqlConnection con = new(_contexto.Conexion))
            {
                using (SqlCommand cmd = new("ActualizarToken", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //parametros
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    //calcular  la fecha + 5minutos
                    DateTime fecha = DateTime.UtcNow.AddMinutes(5);
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    //generar nuevo token
                    var token = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("@Token", token.ToString());
                    con.Open(); // abrir conexion
                    cmd.ExecuteNonQuery(); // ejecutar
                    con.Close();//cierre de conexion

                    //INSERTAR ENVIO DE CORREO

                    Email email = new();
                    //validar
                    if (correo != null)

                        email.Enviar(correo, token.ToString());

                }
            }
        }


        //LISTAR ROLES --
        public List<Rol> ListarRoles()
        {
            var model = new List<Rol>();
            using (var connection = new SqlConnection(_contexto.Conexion))
            {
                connection.Open();//abrtir conexion
                using (SqlCommand cmd = new("ListarRoles", connection))
                {
                    //definir tipo de commandtype
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        //mientras el reader lea
                        while (reader.Read())
                        {
                            //llenar y agregar los campos 
                            var rol = new Rol();
                            rol.RolId = Convert.ToInt32(reader["RolId"]);
                            rol.Nombre = Convert.ToString(reader["Nombre"]);
                            model.Add(rol);
                        }
                    }
                }
            }
            return model;
        }


        //LISTAR USUARIOS--
        public List<Usuario> ListarUsuarios()
        {
            var usuarios = new List<Usuario>();
            using (SqlConnection con = new(_contexto.Conexion))
            {
                using (SqlCommand cmd = new("ListarUsuarios", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //abrir conexion (no hay parametros)
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var usuario = new Usuario
                        {
                            UsuarioId = (int)rdr["UsuarioId"],
                            Nombre = rdr["Nombre"].ToString(),
                            Apellido = rdr["Apellido"].ToString(),
                            Correo = rdr["Correo"].ToString(),
                            Contrasenia = rdr["Contrasenia"].ToString(),
                            RolId = (int)rdr["RolId"], // cast a int
                            NombreUsuario = rdr["NombreUsuario"].ToString(),
                            Estado = (Boolean)rdr["Estado"], //cast a boleano
                            Token = rdr["Token"].ToString(),
                            FechaExpiracion = Convert.ToDateTime(rdr["FechaExpiracion"])
                        };
                        usuarios.Add(usuario);
                    }
                }
            }

            return usuarios;

        }



        //OBTENER ID DE USUARIO EN ESPECIFICO PARA ACTUALIZAR  O ELIMINAR USUARIO
        public Usuario ObtenerUsuarioPorId(int id)
        {
            //instancia de Usuario inicializada
            Usuario usuario = new();
            using (SqlConnection con = new(_contexto.Conexion))
            {
                using (SqlCommand cmd = new("ObtenerUsuarioPorId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //parametros
                    cmd.Parameters.AddWithValue("@UsuarioId", id);
                    con.Open();//abrri con
                    SqlDataReader rdr = cmd.ExecuteReader();

                    //validar
                    if (rdr.Read())
                    {
                        usuario = new Usuario
                        {
                            UsuarioId = id,
                            //Nombre igual a lo que lea el reader y convertirlo a string
                            Nombre = rdr["Nombre"].ToString(),
                            Apellido = rdr["Apellido"].ToString(),
                            Correo = rdr["Correo"].ToString(),
                            Contrasenia = rdr["Contrasenia"].ToString(),
                            RolId = (int)rdr["RolId"], // cast a int
                            NombreUsuario = rdr["NombreUsuario"].ToString(),
                            Estado = (Boolean)rdr["Estado"], //cast a boleano
                            Token = rdr["Token"].ToString(),
                            FechaExpiracion = Convert.ToDateTime(rdr["FechaExpiracion"])
                        };
                    }

                }
            }

            return usuario;
        }






    }
}


