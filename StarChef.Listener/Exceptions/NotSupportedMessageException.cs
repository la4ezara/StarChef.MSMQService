using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Listener.Exceptions
{
    class NotSupportedMessageException : ListenerException
    {
        public NotSupportedMessageException(string message) : base(message)
        {
        }
    }
}
