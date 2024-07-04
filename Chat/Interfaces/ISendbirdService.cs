namespace TucConnect.Data.Interfaces
{

    public class SendBirdOptions
    {
        public string? AppId { get; set; }
        public string? ApiToken { get; set; }
    }
    public interface ISendbirdService
    {
        Task<string> CreateUserAsync(string userId, string nickname);
        Task<string> CreateChannelAsync(string channelName, List<string> userIds);
        Task<bool> SendMessageAsync(string channelUrl, string userId, string message);
    }
}
