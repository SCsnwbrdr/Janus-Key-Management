using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeyVaultExample.Service;
using Microsoft.AspNetCore.Mvc;

namespace KeyVaultExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private MemoryService _memory;
        private AzureServiceBusService _serviceBus;

        public ValuesController(MemoryService memory, AzureServiceBusService serviceBus)
        {
            _memory = memory;
            _serviceBus = serviceBus;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _memory.Memory.ToArray();
        }

        // GET api/values/5
        [HttpGet("Fetch")]
        public void Fetch()
        {
            _serviceBus.GetMessages();
        }

        // GET api/values/5
        [HttpGet("Send")]
        public async Task Send()
        {
            await _serviceBus.SendObject(DateTime.Now.ToLongTimeString() + "Got Here!");
        }
    }
}
