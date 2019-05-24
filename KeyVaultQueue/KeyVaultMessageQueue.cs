using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyVaultQueue
{
    public class KeyVaultMessageQueue: IDisposable
    {
        

        const string ServiceBusConnectionStringFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2}";
        private string _endpoint;
        private string _sharedAccessKeyName;
        private string _queueName;
        private List<KeyToken> _tokens = new List<KeyToken>();
        private string _activeUrl;
        QueueClient sendQueueClient;
        QueueClient receiveQueueClient;

        public KeyVaultMessageQueue(string endpoint, string queueName, string queueAccessKeyName, IEnumerable<string> keyVaultUrls)
        {
            _endpoint = endpoint;
            _sharedAccessKeyName = queueAccessKeyName;
            _queueName = queueName;
            foreach(var url in keyVaultUrls)
            {
                var secretBundle = FetchKeyVault(url).Result;
                _tokens.Add(new KeyToken()
                {
                    Identifier = url,
                    IsPrimary = _tokens.Count == 0,
                    Token = secretBundle.Value
                });
            }
        }

        private async Task<SecretBundle> FetchKeyVault(string keyVaultUrl)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return await keyVaultClient.GetSecretAsync(keyVaultUrl).ConfigureAwait(false);
        }

        public async Task SendString(string rawMessage)
        {
            try
            {
                await PostMessage(rawMessage);
            }
            catch (UnauthorizedException)
            {
                await sendQueueClient.CloseAsync();
                var badToken = _tokens.Find(item => item.IsPrimary);
                var keyToken = _tokens.Find(item => !item.DeadToken && !item.IsPrimary);
                badToken.IsPrimary = false;
                badToken.DeadToken = true;
                if (keyToken == null) throw;
                keyToken.IsPrimary = true;
                await SendString(rawMessage);
                var secretBundle = FetchKeyVault(badToken.Identifier).Result;
                badToken.Token = secretBundle.Value;
                badToken.DeadToken = false;
            }
        }

        private async Task PostMessage(string rawMessage)
        {
            sendQueueClient = new QueueClient(ServiceBusConnectionString(), _queueName);
            var message = new Message(Encoding.UTF8.GetBytes(rawMessage));
            await sendQueueClient.SendAsync(message);
        }


        public void ReadFromQueue(Func<Message,CancellationToken,Task> handler, MessageHandlerOptions options)
        {
            receiveQueueClient = new QueueClient(ServiceBusConnectionString(), _queueName);
            receiveQueueClient.RegisterMessageHandler(handler, options);
        }

        public async Task CompleteAsync(string token)
        {
           await receiveQueueClient.CompleteAsync(token);
        }


        private string ServiceBusConnectionString()
        {
            var activeToken = _tokens.Find(item => item.IsPrimary);
            if (activeToken == null) throw new NullReferenceException("There are no keys listed as a the primary key, either no keys were passed or all keys failed.");
            return string.Format(ServiceBusConnectionStringFormat, _endpoint, _sharedAccessKeyName, activeToken.Token);
        }

        public void Dispose()
        {
            if (!receiveQueueClient.IsClosedOrClosing)
            {
                receiveQueueClient.CloseAsync();
            }
            if (!sendQueueClient.IsClosedOrClosing)
            {
                sendQueueClient.CloseAsync();
            }
        }
    }
}
