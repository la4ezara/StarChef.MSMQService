using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Listener.Exceptions
{
    internal class DataNotSavedException : ListenerException
    {
        public DataNotSavedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DataNotSavedException(string message) : base(message)
        {
        }
    }
}
