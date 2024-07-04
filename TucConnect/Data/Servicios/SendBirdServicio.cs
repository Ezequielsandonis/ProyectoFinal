using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TucConnect.Interfaces;

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




    //ENVIAR MENSAJE
    public async Task<string> SendMessage(string channelUrl, string userId, string message)
    {
        var jsonBody = new
        {
            user_id = userId,
            message
        };

        var content = new StringContent(JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages", content);

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


    //OBTENER LSO MENSAJES  
    public async Task<string> GetChannelMessages(string channelUrl)
    {
        var response = await _httpClient.GetAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/group_channels/{channelUrl}/messages");

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        else
        {
            throw new HttpRequestException($"Failed to get messages from Sendbird channel: {response.StatusCode}");
        }
    }

    //verificar existencia de usuario
    public async Task<bool> UserExists(string userId)
    {
        var response = await _httpClient.GetAsync($"https://api-{_sendbirdAppId}.sendbird.com/v3/users/{userId}");
        return response.IsSuccessStatusCode;
    }


}
