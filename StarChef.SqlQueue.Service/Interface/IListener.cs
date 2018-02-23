namespace StarChef.SqlQueue.Service.Interface
{
    using System.Threading.Tasks;

    public interface IListener
    {
        Task<bool> ExecuteAsync();
        bool CanProcess { get; set; }
    }
}