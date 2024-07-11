using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TucConnect.Interfaces;
using TucConnect.Models.Models;

public class SendbirdService : ISendBirdServicio
{
    //instancias
    private readonly string _sendbirdAppId;
    private readonly string _sendbirdApiToken;
    private readonly HttpClient _httpClient;

    //constructor
    public SendbirdService(string sendbirdAppId, string sendbirdApiToken)
    {
        _sendbirdAppId = sendbirdAppId;
        _sendbirdApiToken = sendbirdApiToken;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Api-Token", _sendbirdApiToken);
    }


    //CREAR UN NUEVO USUSARIO
    public async Task<string> CreateUser(string userId, string nickname)
    {
        var userIdString = userId.ToString();
        var jsonBody = new
        {
            user_id = userIdString,
            nickname,
            profile_url = ""
        };

        var content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/users", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Fallo al crear usuario En sendbird: {response.StatusCode}. Error message: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log or handle the exception 
            throw new HttpRequestException("Error creating Sendbird user", ex);
        }
    }


    //CREAR UN CHAT
    public async Task<string> CreateChatChannel(string userId1, string userId2)
    {
        var jsonBody = new
        {

            name = $"Chat entre {userId1} y {userId2}",
            is_distinct = true, // Crea un canal distinto para los mismos usuarios
            user_ids = new string[] { userId1, userId2 }
            // Puedes incluir más campos opcionales según necesites, como channel_url, cover_url, custom_type, operator_ids, etc.
        };

        var content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create Sendbird chat channel: {response.StatusCode}. Error message: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log or handle the exception as needed
            throw new HttpRequestException("Error creating Sendbird chat channel", ex);
        }
    }




    public async Task<string> SendMessage(string channelUrl, string userId, string message)
    {
        try
        {
            var apiUrl = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages";

            var jsonBody = new
            {
                message_type = "MESG",
                user_id = userId,
                message
            };

            var content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                throw new HttpRequestException($"Fallo al enviar mensaje en Sendbird chat: {response.StatusCode}");
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Error al enviar mensaje en Sendbird chat: {ex.Message}", ex);
        }
    }





    //OBTENER LOS CHATS

    public async Task<string> GetUserChannels(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/users/{userId}/my_group_channels?show_member=true");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get Sendbird user's channels: {response.StatusCode}. Error message: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Log or handle the exception as needed
            throw new HttpRequestException("Error getting Sendbird user's channels", ex);
        }
    }

    //TRAER  ULTIMO MENSAJE 
    public async Task<SendbirdMensaje> GetLastSentMessage(string channelUrl)
    {
        try
        {
            // Obtener todos los mensajes del canal
            var lastMessage = await GetChannelMessages(channelUrl, messageTs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            // Ordenar los mensajes por created_at en orden descendente
            var lastSentMessage = lastMessage.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            return lastSentMessage;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener el último mensaje enviado en Sendbird: {ex.Message}", ex);
        }
    }


    //TRAER LOS MENSAJES
    public async Task<IEnumerable<SendbirdMensaje>> GetChannelMessages(string channelUrl, string channelType = "group_channels", long? messageTs = null)
    {
        // Validar parámetros
        if (string.IsNullOrWhiteSpace(channelUrl))
        {
            throw new ArgumentException("El URL del canal no puede estar vacío.", nameof(channelUrl));
        }
        if (string.IsNullOrWhiteSpace(channelType))
        {
            throw new ArgumentException("El tipo de canal no puede estar vacío.", nameof(channelType));
        }

        // Construir la URL de la solicitud
        var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/{channelType}/{channelUrl}/messages";

        // Agregar parámetros de consulta
        var queryParameters = new List<string>();

        if (messageTs != null)
        {
            queryParameters.Add($"message_ts={messageTs.Value}");
        }

        if (queryParameters.Any())
        {
            url += "?" + string.Join("&", queryParameters);
        }

        // Realizar la solicitud HTTP GET
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserializar la respuesta JSON
            var sendbirdResponse = JsonSerializer.Deserialize<SendbirdResponse>(responseBody);

            if (sendbirdResponse != null && sendbirdResponse.Messages != null)
            {
                return sendbirdResponse.Messages;
            }
            else
            {
                return Enumerable.Empty<SendbirdMensaje>();
            }
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to get messages from Sendbird channel: {response.StatusCode}. Response: {responseBody}");
        }
    }



    //verificar existencia de usuario
    public async Task<bool> UserExists(string userId)
    {
        var response = await _httpClient.GetAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/users/{userId}");
        return response.IsSuccessStatusCode;
    }

    //msg automatico
    public async Task<string> SendAdminMessage(string channelUrl, string message)
    {
        try
        {
            var adminUserId = "730865"; // ID del usuario administrador en Sendbird
            var sendMessageRequest = new
            {
                message_type = "ADMM",
                message = message,
                admin_id = adminUserId,
                is_silent = true // Opcional: Para enviar el mensaje de forma silenciosa
            };

            var sendMessageJson = JsonSerializer.Serialize(sendMessageRequest);
            var content = new StringContent(sendMessageJson, Encoding.UTF8, "application/json");

            // Construir la URL para la solicitud
            var url = $"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to send admin message: {response.StatusCode}. Response: {responseBody}");
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }



    }
}
