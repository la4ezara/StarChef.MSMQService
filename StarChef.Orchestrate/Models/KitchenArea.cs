using System;
using System.Collections.Generic;
using System.Linq;
namespace StarChef.Orchestrate.Models
{
    public class KitchenArea
    {
        public int ProductPartId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
