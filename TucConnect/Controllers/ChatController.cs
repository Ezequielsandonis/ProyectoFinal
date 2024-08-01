using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TucConnect.Data;
using TucConnect.Data.Servicios;
using TucConnect.Interfaces;
using TucConnect.Models.Models;
using TucConnect.Models.Models.ViewModels;

namespace TucConnect.Controllers
{
    public class ChatController : Controller
    {
        private readonly Contexto _contexto;   //objeto para la Conexion a la base de datos s
        private readonly ISendBirdServicio _sendbirdService;
        private readonly UsuarioServicio _usuarioServicio; // Referencia a la clase de servicio
        private readonly PostServicio _postServicio;
        private readonly IHubContext<ChatHub> _hubContext;


        public ChatController(Contexto con, ISendBirdServicio sendbirdService, IHubContext<ChatHub> hubContext)
        {
            _contexto = con;
            _sendbirdService = sendbirdService;
            _usuarioServicio = new UsuarioServicio(con);
            _postServicio = new PostServicio(con);
            _hubContext = hubContext;
        }





        public async Task<IActionResult> IniciarChat(int postId)
        {
            try
            {
                // Obtener el propietario del post
                var ownerUser = _postServicio.ObtenerUsuarioPorPostId(postId);
                if (ownerUser == null)
                {
                    return RedirectToAction("Error", "Home");
                }



                // Verificar si el propietario del post ya existe como usuario en Sendbird
                if (!await _sendbirdService.UserExists(ownerUser.UsuarioId.ToString()))
                {
                    await _sendbirdService.CreateUser(ownerUser.UsuarioId.ToString(), ownerUser.NombreUsuario);
                }

                // Obtener el usuario actual
                var userIdClaim = User.FindFirst("UsuarioId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var usuario = _usuarioServicio.ObtenerUsuarioPorId(userId);
                    if (usuario == null)
                    {
                        return RedirectToAction("Error", "Home");
                    }

                    // Obtener el correo del propietario del post
                    var ownerUserDetails = _usuarioServicio.ObtenerUsuarioPorId(ownerUser.UsuarioId);
                    if (ownerUserDetails == null || string.IsNullOrEmpty(ownerUserDetails.Correo))
                    {
                        return RedirectToAction("Error", "Home");
                    }


                    // Verificar si el usuario actual ya existe como usuario en Sendbird
                    if (!await _sendbirdService.UserExists(userId.ToString()))
                    {
                        await _sendbirdService.CreateUser(userId.ToString(), usuario.NombreUsuario);
                    }

                    // Obtener los canales del usuario actual desde Sendbird
                    var channelsJson = await _sendbirdService.GetUserChannels(userId.ToString());
                    Console.WriteLine("Channels JSON: " + channelsJson); // Debugging

                    // Deserializar la respuesta JSON de Sendbird
                    var channelsResponse = JsonSerializer.Deserialize<SendbirdChannelsResponse>(channelsJson);
                    Console.WriteLine("Channels Response: " + JsonSerializer.Serialize(channelsResponse)); // Debugging

                    if (channelsResponse != null && channelsResponse.Channels != null)
                    {
                        var channels = channelsResponse.Channels;

                        // Verificar si ya existe un canal entre los dos usuarios
                        var existingChannel = channels.FirstOrDefault(c =>
                        {
                            // Obtener los IDs de usuario del canal
                            var memberUserIds = c.Members?.Select(m => m.UserId).ToList();

                            // Verificar si ambos usuarios están en el canal
                            return memberUserIds != null &&
                                   memberUserIds.Contains(ownerUser.UsuarioId.ToString()) &&
                                   memberUserIds.Contains(userId.ToString());
                        });

                        if (existingChannel != null)
                        {
                            // Redirigir al chat existente
                            return RedirectToAction("Index", new { channelUrl = existingChannel.ChannelUrl });
                        }
                        else
                        {
                            // Si no existe, crear un nuevo canal de chat entre los dos usuarios
                            var channelResponse = await _sendbirdService.CreateChatChannel(ownerUser.UsuarioId.ToString(), userId.ToString());
                            // Extraer el channelUrl del response
                            var channelUrl = ExtractChannelUrl(channelResponse);

                            if (!string.IsNullOrEmpty(channelUrl))
                            {
                                // Enviar mensaje automático de administrador
                                await _sendbirdService.SendAdminMessage(channelUrl, "Se inició un nuevo chat");
                                await _sendbirdService.SendMessage(channelUrl, userId.ToString(), "¡Comienza a chatear!");
                                // Envío de correo electrónico al propietario del post

                                Email email = new();
                                //validar

                                if (ownerUser != null)
                                    email.EnviarNotificacion(ownerUserDetails.Correo, channelUrl);


                                return RedirectToAction("Index");
                            }
                            else
                            {
                                // Error al crear el canal de chat
                                ViewBag.Error = "Fallo al crear el canal de chat.";
                                return View("Error");
                            }
                        }
                    }
                    else
                    {
                        // Error al obtener la respuesta de los canales desde Sendbird
                        ViewBag.Error = "Fallo al obtener los canales del usuario desde Sendbird.";
                        return View("Error");
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"Error de solicitud HTTP: {ex.Message}";
                return View("Error");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }

        //extraer el channel url
        private string ExtractChannelUrl(string channelResponse)
        {
            try
            {
                // Deserializar la respuesta JSON para extraer el channelUrl
                var jsonResponse = JsonSerializer.Deserialize<SendbirdChannel>(channelResponse);

                // Verificar si se deserializó correctamente y si contiene el channelUrl
                if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.ChannelUrl))
                {
                    return jsonResponse.ChannelUrl;
                }
                else
                {
                    // Manejar el caso donde no se pueda extraer el channelUrl
                    return null; // o lanzar una excepción, según tu flujo
                }
            }
            catch (JsonException ex)
            {
                // Manejar cualquier error de deserialización
                Console.WriteLine($"Error al deserializar la respuesta JSON: {ex.Message}");
                return null; // o lanzar una excepción, según tu flujo
            }
        }

        //LISTAR MIS CHATS
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener usuario autenticado
                var userIdClaim = User.FindFirst("UsuarioId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Manejar el caso donde el usuario no está autenticado o el id no es válido
                    return RedirectToAction("Error", "Home");
                }

                // Obtener canales de chat del usuario desde Sendbird
                var channelsJson = await _sendbirdService.GetUserChannels(userId.ToString());
                // Manejar el caso donde el usuario no existe en Sendbird
                if (channelsJson == null)
                {
                    return View("Index");
                }


                // Deserializar la respuesta JSON a una instancia de SendbirdChannelsResponse
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Ignorar mayúsculas y minúsculas en los nombres de propiedad
                };

                var channelsResponse = JsonSerializer.Deserialize<SendbirdChannelsResponse>(channelsJson, options);

                // Verificar si channelsResponse es nulo o vacío
                if (channelsResponse == null || channelsResponse.Channels == null || !channelsResponse.Channels.Any())
                {
                    // Redirigir a una vista que maneje el caso de que el usuario no tenga chats
                    return View("Index");
                }



                return View(channelsResponse.Channels);
            }
            catch (JsonException jsonEx)
            {
                ViewBag.Error = $"Error de JSON al deserializar los canales: {jsonEx.Message}";
                return View("Error");
            }
            catch (Exception ex)
            {
                // Manejar el error según sea necesario
                ViewBag.Error = $"Error al obtener los canales de chat: {ex.Message}";
                return View("Error");
            }
        }


        //VER CHAT

        public async Task<IActionResult> VerChat(string channelUrl)
        {
            try
            {
                var userIdClaim = User.FindFirst("UsuarioId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return RedirectToAction("Error", "Home");
                }

                // Obtener mensajes del canal desde Sendbird
                var messages = await _sendbirdService.GetChannelMessages(channelUrl, "group_channels", messageTs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                // Crear un ViewModel para pasar la información necesaria a la vista
                var viewModel = new ChatViewModel
                {
                    ChannelUrl = channelUrl,
                    Messages = messages,
                    UserId = userId
                };



                return PartialView("~/Views/Shared/Partials/_VerChat.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción y mostrar un mensaje de error en la vista
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        //ACTUALIZAR MENSAJE
        [HttpGet]
        public async Task<IActionResult> ActualizarMensajes(string channelUrl)
        {
            try
            {
                // Aquí deberías implementar la lógica para obtener los mensajes actualizados del canal específico
                var messages = await _sendbirdService.GetChannelMessages(channelUrl, "group_channels", messageTs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                // Devolver la vista parcial o los datos de mensajes actualizados
                return PartialView("~/Views/Shared/Partials/_MessagesPartial.cshtml", messages);
            }
            catch (Exception ex)
            {
                // Manejar cualquier error y devolver una respuesta adecuada
                return BadRequest($"Error al actualizar los mensajes: {ex.Message}");
            }
        }

        //ENVIAR MENSAJES


        [HttpPost]
        public async Task<IActionResult> SendMessage(string channelUrl, string message)
        {
            try
            {
                // Obtener usuario autenticado
                var userIdClaim = User.FindFirst("UsuarioId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Manejar el caso donde el usuario no está autenticado o el id no es válido
                    return RedirectToAction("Error", "Home");
                }

                // Enviar el mensaje al canal utilizando el servicio Sendbird
                await _sendbirdService.SendMessage(channelUrl, userId.ToString(), message);

                // Notificar a los clientes conectados que los mensajes han sido actualizados
                await _hubContext.Clients.Group(channelUrl).SendAsync("ReceiveMessage", userId.ToString(), message);
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                // Manejar error de solicitud HTTP específicamente
                ViewBag.Error = $"Error al enviar mensaje: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Manejar otros errores generales
                ViewBag.Error = $"Ocurrió un error: {ex.Message}";
            }



            // Redirigir a la vista del chat en caso de error
            return PartialView("~/Views/Shared/Partials/_MessagesPartial.cshtml", Enumerable.Empty<SendbirdMensaje>()); ;
        }







        //NUEVOS MENSAJES
        public async Task<IActionResult> ObtenerNuevosMensajes(string channelUrl, long ultimoMensajeTimestamp)
        {
            try
            {
                // Llamar al servicio Sendbird para obtener los mensajes del canal
                var mensajes = await _sendbirdService.GetChannelMessages(channelUrl);




                // Filtrar los nuevos mensajes basados en el timestamp del último mensaje recibido
                var nuevosMensajes = mensajes.Where(m => m.CreatedAt > ultimoMensajeTimestamp).ToList();




                // Devolver el número de mensajes nuevos como JSON
                return Json(nuevosMensajes.Count);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener nuevos mensajes: {ex.Message}");
            }
        }
    }
}
