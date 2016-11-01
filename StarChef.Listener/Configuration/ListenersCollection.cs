using System;
using System.Configuration;

namespace StarChef.Listener.Configuration
{
    public sealed class ListenersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ListenerElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((ListenerElement) element).Handler;
        }
    }
}