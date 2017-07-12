using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Common.Types
{
    public class ImportTypeSettings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool AutoCalculateCost { get; set; }
        public bool AutoCalculateIntolerance { get; set; }
    }
}
