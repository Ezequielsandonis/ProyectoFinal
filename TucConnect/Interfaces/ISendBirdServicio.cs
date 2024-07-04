
namespace TucConnect.Interfaces
{
    public interface ISendBirdServicio
    {
         Task<string> CreateUser( string userId, string nickname);
        Task<string> CreateChatChannel( string userId1, string userId2);
        Task<string> SendMessage(string channelUrl, string userId, string message);
        Task<string> GetUserChannels( string userId);
        Task<string> GetChannelMessages(string channelUrl);
        Task<bool> UserExists(string v);
    }
}
