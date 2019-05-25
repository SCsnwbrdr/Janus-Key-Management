using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JanusKeyManagement;
using Microsoft.Azure.ServiceBus;

namespace KeyVaultExample.Service
{
    public class AzureServiceBusService : IDisposable, IAzureServiceBusService
    {
        private readonly MemoryService _memoryService;
        private readonly IJanusKeyEngine _janusKeyEngine;
        const string ServiceBusConnectionStringFormat = "Endpoint={0};SharedAccessKeyName={1};SharedAccessKey={2}";
        private string _endpoint;
        private string _sharedAccessKeyName;
        private string _queueName;
        private QueueClient sendQueueClient;
        private QueueClient receiveQueueClient;

        public AzureServiceBusService(MemoryService memory, string endpoint, string queueName, string queueAccessKeyName, IJanusKeyEngine janusKeyEngine)
        {
            _memoryService = memory;
            _endpoint = endpoint;
            _sharedAccessKeyName = queueAccessKeyName;
            _queueName = queueName;
            _janusKeyEngine = janusKeyEngine;
            sendQueueClient = RegenerateConnection();
            receiveQueueClient = RegenerateConnection();
        }

        private QueueClient RegenerateConnection()
        {
            return new QueueClient(ServiceBusConnectionString(), _queueName);
        }

        public async Task SendString(string rawMessage)
        {
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(rawMessage));
                await sendQueueClient.SendAsync(message);
            }
            catch (UnauthorizedException)
            {
                await sendQueueClient.CloseAsync();
                _janusKeyEngine.RotateToken();
                sendQueueClient = RegenerateConnection();
                await SendString(rawMessage);
                await _janusKeyEngine.RefreshDeadTokens();
            }
        }

        public async Task GetMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };
            
            receiveQueueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.

            _memoryService.Memory.Add($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await receiveQueueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private string ServiceBusConnectionString()
        {
            return string.Format(ServiceBusConnectionStringFormat, _endpoint, _sharedAccessKeyName, _janusKeyEngine.ActiveToken.Token);
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
