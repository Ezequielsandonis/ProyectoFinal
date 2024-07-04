using SendBird;

namespace TucConnect.Data.Interfaces
{
    public class SendbirdService : ISendbirdService
    {
        private readonly SendBirdOptions _sendBirdOptions;

        public SendbirdService(IOptions<SendBirdOptions> sendBirdOptions)
        {
            _sendBirdOptions = sendBirdOptions.Value;

            // Configurar las credenciales de Sendbird
            SendbirdConfiguration.SetAppId(_sendBirdOptions.AppId);
            SendbirdConfiguration.SetApiToken(_sendBirdOptions.ApiToken);
        }
    }

  
}
