using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyVaultQueue
{
    public class KeyVaultMessageQueue
    {
        

        const string ServiceBusConnectionStringFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2}";
        private string _endpoint;
        private string _sharedAccessKeyName = "SendQueueMessage";

        public KeyVaultMessageQueue(string endpoint, string queueAccessKeyName, string primaryKeyVaultUrl, string secondaryKeyVaultUrl)
        {
            _endpoint = endpoint;
            _sharedAccessKeyName = queueAccessKeyName;
        }

        public async Task<SecretBundle> FetchKeyVault(string keyVaultUrl)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            return await keyVaultClient.GetSecretAsync(keyVaultUrl).ConfigureAwait(false);
        }

        public async Task SendString(string queueName, string rawMessage)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, queueName);
            var message = new Message(Encoding.UTF8.GetBytes(rawMessage));
            await queueClient.SendAsync(message);
            await queueClient.CloseAsync();
        }

        public async Task ReadFromQueue(string queueName, Func<Message,CancellationToken,Task> handler, MessageHandlerOptions options)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, queueName);
            SetupAndFetchMessages(queueClient, handler, options);
            await queueClient.CloseAsync();
        }

        private void SetupAndFetchMessages(QueueClient client, Func<Message, CancellationToken, Task> handler, MessageHandlerOptions options)
        {
            client.RegisterMessageHandler(handler, options);
        }
        private string ServiceBusConnectionString(string sharedAccessKey)
        {
            return string.Format(ServiceBusConnectionStringFormat, _endpoint, _sharedAccessKeyName, sharedAccessKey);
        }
    }
}
