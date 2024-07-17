using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TucConnect.Data;
using TucConnect.Data.Enums;
using TucConnect.Data.Servicios;
using TucConnect.Models;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;
using X.PagedList.Extensions;

namespace TucConnect.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly Contexto _contexto;
        private readonly UsuarioServicio _usuarioServicio;
        private readonly PostServicio _postServicio;

        //constructor con los servicios 
        public UsuarioController( Contexto contexto)
        {
            _contexto = contexto;
            _usuarioServicio = new UsuarioServicio(contexto);
            _postServicio = new PostServicio(contexto);
        }

        //METODO PARA MOSTRAR EL PERFIL DE USUARIO
        //el usuario debe estar autorizado (inició sesion) para ingresar a esta pagina
        [Authorize]
        public IActionResult Perfil()
        {
            //control de erroes
            try
            {
                int userId = 0;
                var userIdClaim = User.FindFirst("UsuarioId");
                //validar y guardar el id del usuario en una variable, Busca el id del usuario
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parseUserId))
                    userId = parseUserId;
                //instancia de Usuario para llamar al metodo ObtenerUsuarioPorId
                Usuario usuario = _usuarioServicio.ObtenerUsuarioPorId(userId);
                return View(usuario);
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

          
        }

        //Mostrar post de usuarios

        [Authorize]
        public IActionResult MisPost(int? pagina)
        {

            try
            {

                int userId = 0;
                var userIdClaim = User.FindFirst("UsuarioId");
                //validar y guardar el id del usuario en una variable, Busca el id del usuario
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parseUserId))
                    userId = parseUserId;

                var posts = _postServicio.ListarPostPorUsuarioId(userId);

                // validar que el usuario tenga posteos
                if (posts.Count == 0)
                {
                    ViewBag.Error = $"Aún no tienes publicaciones.";
                }

                //paginacion
                int pageSize = 6;
                //si no hay nada en la variable Numbre , se asigna el 1
                int pageNumber = (pagina ?? 1);
                return View(posts.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }



        //METODO PARA ACTUALIZAR PEFRIL
        [HttpPost]
        public ActionResult ActualizarPerfil(Usuario model) //se reciben los datos del modelo de Usuario
        {
            //control de errores
            try
            {
                using (SqlConnection con = new SqlConnection(_contexto.Conexion))
                {
                    //estructura using command que recibe los 2 parametros (conexion y procedimiento)
                    using (SqlCommand cmd = new("ActualizarPerfil", con))
                    {
                        //definir tipo de procedimiento
                        cmd.CommandType = CommandType.StoredProcedure;
                        //parametros a actualizar
                        cmd.Parameters.AddWithValue("@UsuarioId", model.UsuarioId);
                        cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", model.Apellido);
                        cmd.Parameters.AddWithValue("@Correo", model.Correo);

                        con.Open(); //abrir conexion
                        cmd.ExecuteNonQuery(); //ejecutar cmd
                    }
                }
                return RedirectToAction("Perfil");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View("Perfil");
            }
          
        }



        //METODO PARA ELIMINAR PERFIL
        [HttpPost]
        public ActionResult EliminarCuenta()
        {
            //control de erores
            try
            {
                //definir cual usuario esta autenticado para eliminar la cuenta
                int userId = 0;
                var userIdClaim = User.FindFirst("UsuarioId");
                //validar y guardar el id del usuario en una variable
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parseUserId))
                    userId = parseUserId;

                using (SqlConnection con = new SqlConnection(_contexto.Conexion))
                {
                    using (SqlCommand cmd = new("EliminarUsuario", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UsuarioId", userId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                //LLamar al metodo SignOutAsync para Cerrar la sesion
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return RedirectToAction("Perfil");
            }
          
        }

       
    }
}
 
 