using Fourth.StarChef.Invariables;
using System;

namespace StarChef.MSMQService
{
    public class MessageProcessEventArgs : EventArgs
    {
        public UpdateMessage Message { get; private set; }

        public MessageProcessStatus Status { get; private set; }

        public MessageProcessEventArgs(MessageProcessStatus status) {
            Status = status;
        }

        public MessageProcessEventArgs(UpdateMessage msg, MessageProcessStatus status) : this(status) {
            Message = msg;
        }
    }
}
