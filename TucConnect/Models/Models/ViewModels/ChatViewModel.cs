namespace TucConnect.Models.Models.ViewModels
{
    public class ChatViewModel
    {
        public string ChannelUrl { get; set; }
        public IEnumerable<SendbirdMensaje>  Messages { get; set; }
        public int UserId { get; set; }
    }
}
