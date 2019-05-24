using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KeyVaultQueue;
using Microsoft.Azure.ServiceBus;

namespace KeyVaultExample.Service
{
    public class AzureServiceBusService
    {
        private readonly MemoryService _memoryService;
        private readonly KeyVaultMessageQueue _keyVaultMessageQueue;

        public AzureServiceBusService(MemoryService memory, KeyVaultMessageQueue keyVaultMessageQueue)
        {
            _memoryService = memory;
            _keyVaultMessageQueue = keyVaultMessageQueue;
        }


        public async Task SendObject(string rawMessage)
        {
            await _keyVaultMessageQueue.SendString(rawMessage);
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
            _keyVaultMessageQueue.ReadFromQueue(ProcessMessagesAsync, messageHandlerOptions);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.

            _memoryService.Memory.Add($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await _keyVaultMessageQueue.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

    }
}
