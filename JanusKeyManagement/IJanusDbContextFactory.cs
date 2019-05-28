using System;
using System.Collections.Generic;
using System.Text;

namespace JanusKeyManagement
{
    public interface IJanusDbContextFactory
    {
        string ContextTypeName { get; }
    }
}
