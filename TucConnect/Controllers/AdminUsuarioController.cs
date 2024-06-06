using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TucConnect.Data;
using TucConnect.Data.Servicios;
using TucConnect.Models;
using System.Data.SqlClient;
using X.PagedList;
using System.Data;
using System.Reflection;

namespace MyBlog.Controllers
{
    public class AdminUsuarioController : Controller
    {
        //instancias
        private readonly Contexto _contexto;
        private readonly UsuarioServicio _usuarioServicio;

        //constructor
        public AdminUsuarioController(Contexto contexto)
        {
                _contexto = contexto;
                _usuarioServicio = new UsuarioServicio(contexto);
        }

        //METODO INDEX-LISTAR USUARIOS

        public IActionResult Index(string buscar, int? pagina)
        {
            //control de errores

            try
            {
                //llamar al metodo para listar roles de la clase usuario servicio
                var usuarios = _usuarioServicio.ListarUsuarios();
                //validar que buscar no estre vacio o nulo
                if (!string.IsNullOrEmpty(buscar))

                    //filtrar si el correo no es nulo y contiene la palabra del parametro buscar
                    // o algo coincida con el nombre de usuario  que no sea nulo y que contenga a palabra del parametro buscar
                    //de esta forma se puede buscar por nombre o correo electronico
                    usuarios = usuarios.Where(u => u.Correo != null && u.Correo.Contains(buscar) ||
                    u.NombreUsuario != null && u.NombreUsuario.Contains(buscar)).ToList();
                //ordenar por nombre de usuario
                usuarios = usuarios.OrderBy(u => u.NombreUsuario).ToList();

                //crear una lista y llamar al metodo para listar roles
                List<SelectListItem> roles = _usuarioServicio.ListarRoles().Select(r => new SelectListItem
                {
                    //transoformar el id a string para mostrar el nombre
                    Value = r.RolId.ToString(),
                    Text = r.Nombre
                }).ToList();
                //mostrar en el index
                ViewBag.Roles = roles;

                //paginar resultados
                int pageSize = 10;
                int pageNumber = (pagina ?? 1);
                var usuariosPaginados = usuarios.ToPagedList(pageNumber, pageSize);
                //lista actualizada y pqaginada
                return View(usuariosPaginados);
            }
            catch (Exception ex)
            {

                ViewBag.Error=ex.Message;
                return View();
            }
           
        }



        //METODO CREATE
        public IActionResult Create()
        {
            //control de errores
            try
            {
                //listar roles
                List<SelectListItem> roles = _usuarioServicio.ListarRoles().Select(r => new SelectListItem
                {
                    //transoformar el id a string para mostrar el nombre
                    Value = r.RolId.ToString(),
                    Text = r.Nombre
                }).ToList();
                ViewBag.Roles = roles;

            }
            catch (Exception ex)
            {


                ViewBag.Error = ex.Message;
                return View();
            }

            return View();
        }




        //post -- crear nuevo usuario
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            try
            {
                using (SqlConnection con = new(_contexto.Conexion))
                {
                    using (SqlCommand cmd = new("RegistrarUsuario", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //parametros 
                        cmd.Parameters.AddWithValue("@Nombre",usuario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                        //encriptar contraseña con Bcrypt
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
                        cmd.Parameters.AddWithValue("@Contrasenia", hashedPassword);
                        cmd.Parameters.AddWithValue("@RolId", usuario.RolId);
                        cmd.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                        cmd.Parameters.AddWithValue("@Estado", usuario.Estado);
                        //Generar token
                        var token = Guid.NewGuid();
                        cmd.Parameters.AddWithValue("@Token ",token);
                        //expiracion
                        DateTime fechaExpiracion = DateTime.UtcNow.AddMinutes(5);
                        cmd.Parameters.AddWithValue("@FechaExpiracion", fechaExpiracion);
                         
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                //cargar los datos
                return View(usuario);
            }
        }


        //METODO EDITAR 
        public IActionResult Edit(int id)
        {
            try
            {
                //instancia y llamada al metodo para obtener el usuario
                var usuario = _usuarioServicio.ObtenerUsuarioPorId(id);
                // si es nbulo retorna not found
                if (usuario == null) return NotFound();

                //listar roles
                List<SelectListItem> roles = _usuarioServicio.ListarRoles().Select(r => new SelectListItem
                {
                    //transoformar el id a string para mostrar el nombre
                    Value = r.RolId.ToString(),
                    Text = r.Nombre
                }).ToList();
                ViewBag.Roles = roles;
                return View(usuario);
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();  
            }
         
        }




        //post
        [HttpPost]
        public IActionResult Edit(int id, Usuario usuario)
        {
            //validar que coincidan los id
            if(id!=usuario.UsuarioId) return NotFound();

            try
            {
                using (SqlConnection con = new (_contexto.Conexion))
                {
                    using (SqlCommand cmd = new("ActualizarUsuario",con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //parametros
                        cmd.Parameters.AddWithValue("@UsuarioId", usuario.UsuarioId);
                        cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@RolId", usuario.RolId);
                        cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

                        con.Open();//abrir conexion
                        cmd.ExecuteNonQuery(); //ejecutar
                        

                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }




        //METODO DELETE:Get // cargar los datos y preguntar al usuario siu esta seguro
        public IActionResult Delete(int id)
        {
            try
            {
                //instancia y llamada al metodo para obtener el usuario
                var usuario = _usuarioServicio.ObtenerUsuarioPorId(id);
                //validar que el usuario no sea nulo
                if (usuario == null) return NotFound();
                //si no retornar los valores de usuario
                return View(usuario);
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
                return View();
            }
           
        }

        //post -- confirmar eliminacion de datos
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                //eliminar usuario
                using (SqlConnection con = new(_contexto.Conexion))
                {
                    using (SqlCommand cmd = new("EliminarUsuario",con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //parametros
                        cmd.Parameters.AddWithValue("@UsuarioId",id);
                        con.Open();//abrir conexion
                        cmd.ExecuteNonQuery(); // ejecutar el procedimiento
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {

                ViewBag.Error = e.Message;
                //lamar al metodo del servicio
                return View(_usuarioServicio.ObtenerUsuarioPorId(id));
            }
        }

    }
}
