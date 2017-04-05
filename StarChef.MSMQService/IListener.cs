using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public interface IListener {
        Task ExecuteAsync();
    }
}