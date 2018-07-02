#region usings

using System.Messaging;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Service
{
    public class MsmqListener
    {
        public UpdateMessage PollQueue(string queuePath)
        {
            MessageQueue msMq = new MessageQueue(queuePath);
            {
                msMq.Formatter = new XmlMessageFormatter(new[] {typeof (UpdateMessage)});
                Message message = msMq.Receive();
                if (message != null)
                {
                    return (UpdateMessage) message.Body;
                }
            }
            return null;
        }
    }
}