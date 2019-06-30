using System.Collections.Generic;
using System.Threading.Tasks;

namespace JanusKeyManagement
{
    public interface IJanusKeySet
    {
        KeyToken Active { get; }
        Task Refresh();
        void Rotate();
    }
}
