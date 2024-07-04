using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TucConnect.Data;
using TucConnect.Data.Servicios;
using TucConnect.Interfaces;
using TucConnect.Models;
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

        public ChatController(Contexto con , ISendBirdServicio sendbirdService)
        {
            _contexto = con;
            _sendbirdService = sendbirdService;
            _usuarioServicio = new UsuarioServicio(con);
            _postServicio = new PostServicio(con);
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
                            return RedirectToAction("VerChat", new { channelUrl = existingChannel.ChannelUrl });
                        }
                        else
                        {
                            // Si no existe, crear un nuevo canal de chat entre los dos usuarios
                            var channelResponse = await _sendbirdService.CreateChatChannel(ownerUser.UsuarioId.ToString(), userId.ToString());
                            if (!string.IsNullOrEmpty(channelResponse))
                            {
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
                Console.WriteLine("Channels JSON: " + channelsJson); // Debugging

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

                // Puedes procesar 'channelsResponse.Channels' como sea necesario y pasarlos a la vista
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




        //VER LOS MENSAJES
        public async Task<IActionResult> VerChat(string channelUrl)
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

                // Obtener mensajes del canal desde Sendbird
                var messagesJson = await _sendbirdService.GetChannelMessages(channelUrl);
                var messages = JsonSerializer.Deserialize<IEnumerable<SendbirdMensaje>>(messagesJson);

                // Crear un ViewModel para pasar la información necesaria a la vista
                var viewModel = new ChatViewModel
                {
                    ChannelUrl = channelUrl,
                    Messages = messages,
                    UserId = userId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Manejar el error según sea necesario
                ViewBag.Error = ex.Message;
                return View(); // O redirigir a una vista de error
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

                // Enviar el mensaje al canal
                await _sendbirdService.SendMessage(channelUrl, userId.ToString(), message);

                // Redirigir de vuelta a la vista del chat
                return RedirectToAction("VerChat", new { channelUrl });
            }
            catch (Exception ex)
            {
                // Manejar el error según sea necesario
                ViewBag.Error = ex.Message;
                return RedirectToAction("VerChat", new { channelUrl });
            }
        }

    }
}
