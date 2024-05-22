using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TucConnect.Data.Servicios;
using TucConnect.Data;
using Microsoft.AspNetCore.Authorization;
using Sendbird.Entities;
using System.Data.SqlClient;
using System.Data;
using TucConnect.Models;

namespace TucConnect.Controllers
{


    public class PostController : Controller
    {
        private readonly Contexto _contexto;   //objeto para la Conexion a la base de datos 
        private readonly PostServicio _postServicio; // Referencia a la clase de servicio
        private readonly UsuarioServicio _usuarioServicio; // Referencia a la clase de servicio

        public PostController(Contexto con)
        {
            _contexto = con;
            _postServicio = new PostServicio(con);
            _usuarioServicio = new UsuarioServicio(con);
        }
    
    
        //METODO CREATE

      [Authorize] // Restricciones de vistas
        //get
        public IActionResult Create() // vista para crear un nuevo post
        {
            return View();
        }
        //Post -- solicitud y almacenar en la dbb un nuevo posteo
        [HttpPost]
        [Authorize]
        public IActionResult Create(Post post)
        {
            // control de errores
            try
            {

                //validar usuario autenticado
                int? userId = null;
                var userIdclaim = User.FindFirst("UsuarioId");
                if (userIdclaim != null && int.TryParse(userIdclaim.Value, out int parseUserId)) // validar que no sea nulo y que sea un valor entero

                    userId = parseUserId;


                //Procedimiento almacenado 
                using (var connection = new SqlConnection(_contexto.Conexion))
                {
                    connection.Open();
                    using (var command = new SqlCommand("InsertarPost", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Titulo", post.Titulo);
                        command.Parameters.AddWithValue("@Contenido", post.Contenido);
                        command.Parameters.AddWithValue("@Categoria", post.Categoria.ToString()); //Se lo toma desde el enum donde ya esta definido las categorias -- se lo convierte a string
                        command.Parameters.AddWithValue("@Zona", post.Zona.ToString()); // Convertir la zona a string
                        DateTime fc = DateTime.UtcNow;  //convertir la fecha de creacion a la fecha actual para pasarla como paramatro 
                        command.Parameters.AddWithValue("@FechaCreacion", fc);
                        command.Parameters.AddWithValue("@UsuarioId", userId); // Agregar el UsuarioId
                        command.ExecuteNonQuery(); //ejecutar el procedimiento almacenado
                    }

                }
                return RedirectToAction("Index", "Home"); // Vista index del controlador home
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }


        //METODO UPDATE  -- mostrar lista para actualizar post existente
        [Authorize]
        //get
        public IActionResult Update(int id)
        {
            try
            {
                var post = _postServicio.ObtenerPostPorId(id); // metodo para buscar un post y cargar sus  datos
                return View(post);
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }
        //Post
        [HttpPost]
        [Authorize]
        public IActionResult Update(Post post)
        {

            //control de errores
            try
            {
                //validar usuario autenticado
                int? userId = null;
                var userIdclaim = User.FindFirst("UsuarioId");
                if (userIdclaim != null && int.TryParse(userIdclaim.Value, out int parseUserId)) // validar que no sea nulo y que sea un valor entero

                    userId = parseUserId;



                //Procedimiento almacenado 
                using (var connection = new SqlConnection(_contexto.Conexion))
                {
                    connection.Open();
                    using (var command = new SqlCommand("ActualizarPost", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PostId", post.PostId);
                        command.Parameters.AddWithValue("@Titulo", post.Titulo);
                        command.Parameters.AddWithValue("@Contenido", post.Contenido);
                        command.Parameters.AddWithValue("@Zona", post.Zona.ToString());
                        command.Parameters.AddWithValue("@Categoria", post.Categoria.ToString());
                        command.Parameters.AddWithValue("@UsuarioId", userId);
                        command.ExecuteNonQuery(); //ejecutar el procedimiento almacenado
                    }

                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }




        //DELETE
        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            try
            {
                //Procedimiento almacenado 
                using (var connection = new SqlConnection(_contexto.Conexion))
                {
                    connection.Open();
                    using (var command = new SqlCommand("ElminarPost", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PostId", id);
                        command.ExecuteNonQuery(); //ejecutar el procedimiento almacenado
                    }

                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }


        //DETALLES


        //PUBLICAR COMENTARIO

    }
}
