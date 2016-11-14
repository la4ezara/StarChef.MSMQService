using System;
using System.Text;
using System.Xml;
using Fourth.Orchestration.Model.Recipes;
using Google.ProtocolBuffers;
using Newtonsoft.Json;

namespace StarChef.Listener.Extensions
{
    public static class MessageEventsExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty. </exception>
        public static XmlDocument ToXml(this Events.PriceBandUpdated data)
        {
            var xmlString = new StringBuilder();

            foreach (var priceBand in data.PriceBandsList)
            {
                if (!priceBand.HasId || (!priceBand.HasMinimumPrice && !priceBand.HasMaximumPrice))
                    continue;

                xmlString.Append($"<PriceBand><ProductGuid>{priceBand.Id}</ProductGuid><MinPrice>{priceBand.MinimumPrice}</MinPrice><MaxPrice>{priceBand.MaximumPrice}</MaxPrice></PriceBand>");
            }

            if (xmlString.Length > 0)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml($"<PriceBandList>{xmlString}</PriceBandList>");
                return xmlDoc;
            }

            return null;
        }

        public static string ToJson(this object obj)
        {
            var messageLite = obj as IMessageLite;
            if (messageLite != null)
                return Google.ProtocolBuffers.Extensions.ToJson(messageLite);

            return JsonConvert.SerializeObject(obj);
        }
    }
}
