using System.Collections.Generic;
using System.Threading.Tasks;

namespace JanusKeyManagement
{
    public interface IJanusKeyEngine
    {
        KeyToken ActiveToken { get; }
        KeyToken ActiveCredential { get; }

        Task RefreshDeadTokens();
        void RotateToken();
    }
}
