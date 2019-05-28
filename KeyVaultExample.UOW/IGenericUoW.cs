using KeyVaultExample.UOW.GenericRepository;

namespace KeyVaultExample.UOW
{
    public interface IGenericUoW
    {
        IResiliantRepository<T> Repository<T>() where T : class;

       
    }
}