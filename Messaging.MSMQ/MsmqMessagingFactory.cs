using Messaging.MSMQ.Interface;
using System;
using System.Configuration;

namespace Messaging.MSMQ
{
    public class MsmqMessagingFactory : IMessagingFactory
    {
        private const string _configKeyMQ = "StarChef.MSMQ.Queue";

        private string _queueName = string.Empty;

        private MsmqMessageBus _bus;
        private IMsmqSender _sender;
        public MsmqMessagingFactory()
        {

        }
        public MsmqMessagingFactory(string queueName)
        {
            this._queueName = queueName;
        }
        public MsmqMessagingFactory(IMsmqSender sender)
        {
            this._sender = sender;
        }
        public IMessageBus CreateMessageBus()
        {
            if (this._bus == null)
            {
                if (this._sender == null)
                {
                    try
                    {
                        this._queueName = ConfigurationManager.AppSettings[_configKeyMQ].ToString();
                    }
                    catch (Exception)
                    {
                        throw new Exception("StarChef: Cannot find MSMQ configuration. Key " + _configKeyMQ + " is missing from web.config file.");
                    }

                    this._sender = new MsmqSender(this._queueName);
                }
                this._bus = new MsmqMessageBus(this._sender);
            }

            return _bus;
        }
        public void Close()
        {
            if (this._bus != null)
            {
                this._bus.Dispose();
            }
        }

        ~MsmqMessagingFactory()
        {
            this.Dispose(false);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
