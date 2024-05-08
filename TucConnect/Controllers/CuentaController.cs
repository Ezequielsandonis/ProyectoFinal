using Microsoft.AspNetCore.Mvc;
using TucConnect.Data.Servicios;
using TucConnect.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using TucConnect.Models.ViewModels;
using TucConnect.Models;

namespace TucConnect.Controllers
{
    public class CuentaController : Controller
    {
        private readonly Contexto _contexto;
        private readonly UsuarioServicio _usuarioServicio;

        public CuentaController(Contexto con) //constructor
        {
            _contexto = con;
            _usuarioServicio = new UsuarioServicio(con);
        }

        //REGISTRAR USUARIO 

        public IActionResult Registrar()
        {
            return View();
        }

        //post
        [HttpPost, ValidateAntiForgeryToken] //medida de seguridad contra ataques ccrm

        public IActionResult Registrar(Usuario model)
        {
            if (ModelState.IsValid) //validar que el modelo sea valido
            {
                try
                {
                    using (var connection = new SqlConnection(_contexto.Conexion))
                    {
                        connection.Open();
                        using (var command = new SqlCommand("RegistrarUsuario", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            //parametros

                            command.Parameters.AddWithValue("@Nombre", model.Nombre);
                            command.Parameters.AddWithValue("@Apellido", model.Apellido);
                            command.Parameters.AddWithValue("@Correo", model.Correo);
                            //encriptar contraseña con Bcrypt
                            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Contrasenia);
                            command.Parameters.AddWithValue("@Contrasenia", hashedPassword);
                            command.Parameters.AddWithValue("@NombreUsuario", model.NombreUsuario);
                            //Agregar fecha de expiracion
                            DateTime fechaExpiracion = DateTime.UtcNow.AddMinutes(5);
                            command.Parameters.AddWithValue("@FechaExpiracion", fechaExpiracion);
                            //Generar token
                            var token = Guid.NewGuid();
                            command.Parameters.AddWithValue("@Token", token);

                            command.ExecuteNonQuery(); // ejecutar

                            //ENVIO  DE CORREO
                            Email email = new();
                            //validar
                            if (model.Correo != null)

                                email.Enviar(model.Correo, token.ToString());

                        }
                    }
                    return RedirectToAction("Token"); //vista para activar la cuenta
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) //usuario existente
                    {
                        ViewBag.Error = "El correo y/o Nombre de usuario ya existe";
                    }
                    else
                    {

                        ViewBag.Error = "Error al registrar usuario" + ex.Message;
                    }
                }
            }

            return View(model);
        }




        //METODO PARA EL TOKEN, (ACTIVAR CUENTA )
        public IActionResult Token()
        {
            string token = Request.Query["valor"];

            if (token != null) // validar
            {
                try
                {
                    using (SqlConnection con = new(_contexto.Conexion))
                    {
                        using (SqlCommand cmd = new("ActivarCuenta", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            //parametros
                            cmd.Parameters.AddWithValue("@Token", token);
                            //validar que la fecha de activacion de cuenta sea menor a la fecha de expiracin del token
                            DateTime fechaexpiracion = DateTime.UtcNow.AddMinutes(5);
                            cmd.Parameters.AddWithValue("@Fecha", fechaexpiracion);
                            con.Open();//abrir conexion

                            var resultado = cmd.ExecuteScalar(); //ejecutar como un escalar porque se valida que la cuenta se active o no 
                            int activada = Convert.ToInt32(resultado);
                            //validar si la fecha de expiracion ya venció
                            if (activada == 1)
                            {
                                ViewData["mensaje"] = "Cuenta activada exitosamente";
                            }
                            else
                                ViewData["mensaje"] = "Enlace de activacion expirado";

                            con.Close();//cerrar conexion
                        }
                    }
                }
                catch (Exception e)
                {
                    ViewData["mensaje"] = e.Message;
                    return View();

                }
            }
            else
            {
                ViewData["mensaje"] = "Verifique su correo para activar su cuenta";
                return View();
            }
            return View();
        }


        // LOGIN/mostrar formulario de inicio de sesion mediante validaciones


        public IActionResult Login()
        {

            //validar si ya existe un usuario autenticado
            //uso de autentificacion de asp.net
            ClaimsPrincipal c = HttpContext.User;
            if (c.Identity != null)
            {
                //si esta autenticado
                if (c.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home"); //retorna index de home
            } // si no retorna vista de inicio de sesion
            return View();
        }
        //post
        [HttpPost]
        public async Task<IActionResult> login(LoginViewModel model) //recibe el viewmodel del login
        {
            //validar el modelo
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (SqlConnection con = new SqlConnection(_contexto.Conexion))
                {
                    using (SqlCommand cmd = new("ValidarUsuario", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //parametros
                        cmd.Parameters.AddWithValue("@Correo", model.Correo); //enviamos correo
                        con.Open(); //abrir conexion
                        try
                        {
                            using (var dr = cmd.ExecuteReader()) //ejecutar el procedimiento que hace un select a la tabla
                            {
                                if (dr.Read())
                                {
                                    // se obtiene y compara el campo contrasenia con el modelo con el cual se esta intentando iniciar sesion
                                    bool passwordMatch = BCrypt.Net.BCrypt.Verify(model.Contrasenia, dr["Contrasenia"].ToString());
                                    if (passwordMatch)
                                    {
                                        DateTime fechaexpiracion = DateTime.UtcNow;
                                        //validar
                                        if (!(bool)dr["Estado"] && dr["FechaExpiracion"].ToString() != fechaexpiracion.ToString())
                                        {
                                            //validar que el correo no sea nulo
                                            if (model.Correo != null)
                                                _usuarioServicio.ActualizarToken(model.Correo);
                                            ViewBag.Error = "Su cuenta no fue activada, se reenvió un correo a su cuenta, verifiquela.";
                                        } //si el estado es nulo, pero la fecha de expiracion es valida (no pasaron 10 min y sigue intentando iniciar sesion):
                                        else if (!(bool)dr["Estado"])

                                            ViewBag.Error = "Su cuenta no fue activada, verifique su bandeja de entrada.";
                                        // si todo esta bien:
                                        else
                                        {
                                            //se inicia sesion:
                                            string? nombreUsuario = dr["NombreUsuario"].ToString();
                                            int idUsuario = (int)dr["UsuarioId"];

                                            if (nombreUsuario != null)
                                            {
                                                var claims = new List<Claim>()
                                                {
                                                    //Claim para identificar al usuario por su nombre de usuario
                                                    new Claim(ClaimTypes.NameIdentifier,nombreUsuario),
                                                    //Claim para contener el id de usuario para usarlo en otros controladores
                                                    new Claim("UsuarioId" , idUsuario.ToString())
                                                };

                                                //validar rol de usuario
                                                int rolId = (int)dr["RolId"];
                                                //condicionar el valor del rolId, si es 1 el nombre de usuario se le asigna a administrador, en otro caso a usuario
                                                string rolNombre = rolId == 1 ? "Administrador" : "Usuario";
                                                claims.Add(new Claim(ClaimTypes.Role, rolNombre));

                                                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                                var propiedades = new AuthenticationProperties
                                                //propiedades
                                                {
                                                    AllowRefresh = true,
                                                    IsPersistent = (bool)model.MantenerActivo,
                                                    ExpiresUtc = DateTimeOffset.UtcNow.Add((bool)model.MantenerActivo ? TimeSpan.FromDays(1) : TimeSpan.FromMinutes(5))
                                                };

                                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), propiedades);
                                                return RedirectToAction("Index", "Home");
                                            }
                                        }
                                    }
                                }
                                else
                                    ViewBag.Error = "Correo no registrado";
                                dr.Close(); //cerrar datareader
                            }
                        }
                        finally
                        {

                            if (cmd != null)
                            {
                                cmd.Dispose();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ViewBag.Error = ex.Message;
            }
            return View(model);

        }

        //CIERRE DE SESION  
        public async Task<IActionResult> CerrarSesion()
        {
            //cerrar sesion
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


    }
}
