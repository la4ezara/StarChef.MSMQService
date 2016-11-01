using System;
using System.Collections.Generic;
using System.Configuration;

namespace StarChef.Listener.Configuration
{
    /// <summary>
    /// Configuration section for Azure listeners
    /// </summary>
    /// <remarks>
    /// More information about this in MSDN https://msdn.microsoft.com/en-us/library/2tw134k3.aspx
    /// </remarks>
    public class AzureListenersSection : ConfigurationSection
    {
        [ConfigurationProperty("listeners", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof (ListenersCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ListenersCollection Listeners => (ListenersCollection) base["listeners"];

        public static IDictionary<string, IList<Type>> GetConfiguration(string sectionName = "azureListeners")
        {
            var result = new Dictionary<string, IList<Type>>();
            var config = (AzureListenersSection) ConfigurationManager.GetSection(sectionName);
            foreach (ListenerElement listener in config.Listeners)
            {
                if (result.ContainsKey(listener.Subscription) == false)
                    result.Add(listener.Subscription, new List<Type>());

                var handlerType = Type.GetType(listener.Handler);
                if (handlerType != null && result[listener.Subscription].Contains(handlerType) == false)
                    result[listener.Subscription].Add(handlerType);
            }
            return result;
        }
    }
}