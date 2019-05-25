using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace KeyVaultExample.Service
{
    public interface IAzureServiceBusService
    {
        void Dispose();
        Task GetMessages();
        Task ProcessMessagesAsync(Message message, CancellationToken token);
        Task SendString(string rawMessage);
    }
}