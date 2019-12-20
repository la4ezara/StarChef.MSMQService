using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Fourth.Orchestration.Model.Recipes;
using Google.ProtocolBuffers;
using Newtonsoft.Json;
using StarChef.Listener.Exceptions;
using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;

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

        public static string ToXmlString(this double value)
        {
            return value.ToString("#0.00######"); // the precision came from R9
        }

        public static IEnumerable<XmlDocument> ToSmallXmls(this Events.PriceBandUpdated data, int priceBandBatchSize)
        {   
            var xmlString = new StringBuilder();
            var rowsInBanch = 0;
            foreach (var priceBand in data.PriceBandsList)
            {
                if (!priceBand.HasId || (!priceBand.HasMinimumPrice && !priceBand.HasMaximumPrice))
                    continue;

                xmlString.Append(string.Format("<PriceBand><ProductGuid>{0}</ProductGuid><MinPrice>{1}</MinPrice><MaxPrice>{2}</MaxPrice></PriceBand>",
                    priceBand.Id,
                    priceBand.MinimumPrice.ToXmlString(),
                    priceBand.MaximumPrice.ToXmlString()));
                rowsInBanch++;

                if (rowsInBanch == priceBandBatchSize)
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(string.Format("<PriceBandList>{0}</PriceBandList>", xmlString));

                    rowsInBanch = 0;
                    xmlString.Length = 0;

                    yield return xmlDoc;
                }
            }

            if (rowsInBanch < priceBandBatchSize && xmlString.Length > 0)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(string.Format("<PriceBandList>{0}</PriceBandList>", xmlString));

                yield return xmlDoc;
            }
        }

        public static string ToJson(this object obj)
        {
            var messageLite = obj as IMessageLite;
            if (messageLite != null)
                return Google.ProtocolBuffers.Extensions.ToJson(messageLite);

            return JsonConvert.SerializeObject(obj);
        }

        public static bool IsStarChefEvent(this IMessage message)
        {
            if (message == null) return false;
            dynamic obj = message;

            try
            {
                var sourceValue = obj.Source;
                var source = (SourceSystem)sourceValue;

                return source == SourceSystem.STARCHEF;
            }
            catch (Exception e)
            {
                throw new ListenerException("Cannot identify event's source.", e);
            }
        }
    }
}
