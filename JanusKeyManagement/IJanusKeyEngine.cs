using System.Threading.Tasks;

namespace JanusKeyManagement
{
    public interface IJanusKeyEngine
    {
        KeyToken ActiveToken { get; }

        Task RefreshDeadTokens();
        void RotateToken();
    }
}
