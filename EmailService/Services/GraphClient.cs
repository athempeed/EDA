using EDA.Core.Infrastructure.Messaging.Messages;
using EmailService.Modals;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace EmailService.Services
{
    public class GraphClient : IGraphClient
    {
        private readonly EmailOptions _options;
        private DateTimeOffset _expireTime;
        private  GraphServiceClient _client;

        public GraphServiceClient Client {
            get
            {
                DateTimeOffset currentUTCTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                if (_expireTime.CompareTo(currentUTCTime) < 0)
                {
                    Authenticate();
                }
                    return _client;
            } 
        }

        public GraphClient(IOptionsMonitor<EmailOptions> options)
        {
            _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        }
        private void Authenticate()
        {

            List<string> scopes = new List<string>() { "https://graph.microsoft.com/.default" };
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(_options.ClientID)
            .WithTenantId(_options.TenantID)
            .WithClientSecret(_options.ClientSecret)
            .Build();

            _client =
                new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {
                        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                    var authResult = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();
                    _expireTime = authResult.ExpiresOn;
                        // Add the access token in the Authorization header of the API
                    requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                })
            );
        }

        public async Task SendMail(EmailMessage message)
        {
            var messageToSend = new Message
            {
                Subject = message.Subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = message.Body
                },
                
                ToRecipients = message.To?.Split(",").ToList().Select(mail =>
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = mail
                    }
                }),
                CcRecipients = message.Cc?.Split(",").ToList().Select(mail =>
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = mail
                    }
                }),
                BccRecipients = message.Bcc?.Split(",").ToList().Select(mail =>
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = mail
                    }
                })
            };
            await Client.Users[_options.SendAs].SendMail(messageToSend, false).Request().PostAsync();
        }
    }
}
