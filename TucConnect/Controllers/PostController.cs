using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using TucConnect.Data;
using TucConnect.Data.Servicios;
using TucConnect.Models;
using TucConnect.Models.ViewModels;

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
        //get 
        public IActionResult Details(int id)
        {
            //control de errores
            try
            {
                //Datos necesarios 
                var post = _postServicio.ObtenerPostPorId(id);
                var comentarios = _postServicio.ObtenerComentariosPorPostId(id);
                var usuarioCreador = _postServicio.ObtenerNombreUsuarioPorPostId(id);
                comentarios = _postServicio.ObtenerComentariosHijos(comentarios);
                comentarios = _postServicio.ObtenerComentariosNietos(comentarios);

                var model = new PostDetallesViewModel // parametros del ViewModel
                {
                    Post = post,

                    //obtener el nombre del creador del post
                    UsuarioCreador = usuarioCreador,

                    ComentariosPrincipales = comentarios.Where(c => c.ComentarioPadreId == null && c.ComentarioAbueloId == null).ToList(),
                    //se llena la lista solo de comentarios principales
                    ComentariosHijos = comentarios.Where(c => c.ComentarioPadreId != null && c.ComentarioAbueloId == null).ToList(),
                    //se llena la lista solo de comentarios hijos
                    ComentariosNietos = comentarios.Where(c => c.ComentarioPadreId != null).ToList(),
                    //se llena la lista solo de comentarios nietos

                    PostRecientes = _postServicio.ObtenerPosts().Take(10).ToList()
                    //Se llena la lista con los 10 post mas recientes
                };

                return View(model); // se retorna una publicacion en especifico, un modelo 
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }

        }


        //PUBLICAR COMENTARIO

        //METODO PARA PUBLICAR COMENTARIO
        [HttpPost]
        public IActionResult AgregarComentario(int postId, string comentario, int? comentarioPadreId)
        {
            try
            //Validar que el comentario no este vacio
            {
                if (string.IsNullOrWhiteSpace(comentario))
                {
                    ViewBag.Error = "El comentario esta vacio";
                    return RedirectToAction("Details", "Post", new { id = postId });
                }

                //validar usuario autenticado
                int? userId = null;
                var userIdclaim = User.FindFirst("UsuarioId");
                if (userIdclaim != null && int.TryParse(userIdclaim.Value, out int parseUserId)) // validar que no sea nulo y que sea un valor entero

                    userId = parseUserId;

                DateTime fechaPublicacion = DateTime.UtcNow;

                using (SqlConnection con = new(_contexto.Conexion))
                {
                    //procedimiento almacenado para guardar en la db
                    using (SqlCommand cmd = new("AgregarComentario", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Contenido", SqlDbType.VarChar).Value = comentario;//insertar los parametros del procedimiento
                        cmd.Parameters.Add("@FechaCreacion", SqlDbType.DateTime2).Value = fechaPublicacion;
                        cmd.Parameters.Add("@PostId", SqlDbType.Int).Value = postId;
                        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@ComentarioPadreId", SqlDbType.Int).Value = comentarioPadreId ?? (object)DBNull.Value; // validar que no sea nulo convirtiendolo a objeto 
                        con.Open();
                        cmd.ExecuteNonQuery(); // ejecutar el procedimiento 
                        con.Close();
                    }

                    return RedirectToAction("Details", "Post", new { id = postId }); //retornar a la vista
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return RedirectToAction("Details", "Post", new { id = postId });
            }
        }

    }
}
