using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.MSMQService.Interface
{
    public interface IServiceRunner : IDisposable
    {
        void Start();
        void Stop();
        void Pause();
        void Continue();
    }
}
