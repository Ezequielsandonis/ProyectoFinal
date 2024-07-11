
using TucConnect.Models.Models;

namespace TucConnect.Interfaces
{
    public interface ISendBirdServicio
    {
         Task<string> CreateUser( string userId, string nickname);
        Task<string> CreateChatChannel( string userId1, string userId2);
        Task<string> SendMessage(string channelUrl, string userId, string message);
        Task<string> GetUserChannels( string userId);

        Task<SendbirdMensaje> GetLastSentMessage(string channelUrl);
        Task<IEnumerable<SendbirdMensaje>> GetChannelMessages(string channelUrl, string channelType = "group_channels", long? messageTs = null);
        Task<bool> UserExists(string v);

        Task<string> SendAdminMessage(string channelUrl, string message);
    }
}
