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
    public class KeyVaultMessageQueue
    {
        

        const string ServiceBusConnectionStringFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2}";
        private string _endpoint;
        private string _sharedAccessKeyName;
        private string _queueName;
        private List<KeyToken> _tokens = new List<KeyToken>();
        private string _activeUrl;

        public KeyVaultMessageQueue(string endpoint, string queueName, string queueAccessKeyName, List<string> keyVaultUrls)
        {
            _endpoint = endpoint;
            _sharedAccessKeyName = queueAccessKeyName;
            _queueName = queueName;
            foreach(var url in keyVaultUrls)
            {
                var secretBundle = FetchKeyVault(url).Result;
                _tokens.Add(new KeyToken()
                {
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
            var queueClient = new QueueClient(ServiceBusConnectionString(), _queueName);
            var message = new Message(Encoding.UTF8.GetBytes(rawMessage));
            await queueClient.SendAsync(message);
            await queueClient.CloseAsync();
        }

        public async Task ReadFromQueue(Func<Message,CancellationToken,Task> handler, MessageHandlerOptions options)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString(), _queueName);
            SetupAndFetchMessages(queueClient, handler, options);
            await queueClient.CloseAsync();
        }

        private void SetupAndFetchMessages(QueueClient client, Func<Message, CancellationToken, Task> handler, MessageHandlerOptions options)
        {
            client.RegisterMessageHandler(handler, options);
        }
        private string ServiceBusConnectionString()
        {
            var activeToken = _tokens.Find(item => item.IsPrimary);
            if (activeToken == null) throw new NullReferenceException("There are no keys listed as a the primary key, either no keys were passed or all keys failed.");
            return string.Format(ServiceBusConnectionStringFormat, _endpoint, _sharedAccessKeyName, activeToken.Token);
        }
    }
}
