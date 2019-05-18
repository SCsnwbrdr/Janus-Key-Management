using System;
using System.Collections.Generic;
using System.Text;

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
