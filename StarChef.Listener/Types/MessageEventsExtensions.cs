using System.Text;
using System.Xml;
using Fourth.Orchestration.Model.Recipes;

namespace StarChef.Listener.Types
{
    public static class MessageEventsExtensions
    {
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
    }
}
