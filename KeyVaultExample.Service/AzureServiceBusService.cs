using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace KeyVaultExample.Service
{
    public class AzureServiceBusService
    {
        const string ServiceBusConnectionString = "Endpoint=sb://jplservice.servicebus.windows.net/;SharedAccessKeyName=SendQueueMessage;SharedAccessKey=T9qinDHZzrG+0IogTwuvDE1/tBxWAL69Lc2/t8FKozI=";
        const string QueueName = "mainqueue";
        static IQueueClient queueClient;
        private readonly MemoryService _memoryService;

        public AzureServiceBusService(MemoryService memory)
        {
            _memoryService = memory;
        }


        public async Task SendObject(string rawMessage)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            var message = new Message(Encoding.UTF8.GetBytes(rawMessage));
            await queueClient.SendAsync(message);
            await queueClient.CloseAsync();
        }

        public async Task GetMessages()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            FetchMethodsInLoop();
            await queueClient.CloseAsync();
        }

        private void FetchMethodsInLoop()
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

            // Register the function that processes messages.
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            _memoryService.Memory.Add($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            
            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
