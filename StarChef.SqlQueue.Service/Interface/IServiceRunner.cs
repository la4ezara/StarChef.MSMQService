namespace StarChef.SqlQueue.Service.Interface
{
    using System;

    public interface IServiceRunner : IDisposable
    {
        void Start();
        void Stop();
        void Pause();
        void Continue();
    }
}
