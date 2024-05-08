using System.Data.SqlClient;
using System.Data;

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


        //LISTAR USUARIOS--


        //OBTENER ID DE USUARIO EN ESPECIFICO PARA ACTUALIZAR  O ELIMINAR USUARIO






    }
}


