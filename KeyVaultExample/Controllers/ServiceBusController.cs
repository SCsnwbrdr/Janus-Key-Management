using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KeyVaultExample.Service;
using Microsoft.AspNetCore.Mvc;

namespace KeyVaultExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBusController : ControllerBase
    {
        private MemoryService _memory;
        private IAzureServiceBusService _serviceBus;

        public ServiceBusController(MemoryService memory, IAzureServiceBusService serviceBus)
        {
            _memory = memory;
            _serviceBus = serviceBus;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var values = _memory.Memory.ToArray();
            _memory.Memory.Clear();
            return values;
        }

        // GET api/values/5
        [HttpGet("Fetch")]
        public async Task<ActionResult<string>> FetchAsync()
        {
            await _serviceBus.GetMessages();
            return "Listening to Queue";
        }

        // GET api/values/5
        [HttpGet("Send")]
        public async Task<ActionResult<string>> Send()
        {
            var stringtoSendToQueue = DateTime.Now.ToLongTimeString() + " Got Here!";
            await _serviceBus.SendString(stringtoSendToQueue);
            return "Sent: " + stringtoSendToQueue;
        }
    }
}
