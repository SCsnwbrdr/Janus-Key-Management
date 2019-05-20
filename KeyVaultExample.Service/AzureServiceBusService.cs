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

        public async Task GetMessages(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions options)
        {
            await _keyVaultMessageQueue.ReadFromQueue(handler, options);
        }

    }
}
