using System.Collections.Generic;

namespace KeyVaultExample.Service
{
    public class MemoryService
    {
        public MemoryService()
        {

        }

        public List<string> Memory { get; set; } = new List<string>();
    }
}
