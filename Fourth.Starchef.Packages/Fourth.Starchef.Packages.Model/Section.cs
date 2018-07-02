#region usings

using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    public class Section
    {
        public Section()
        {
            Items = new Collection<Item>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public ICollection<Item> Items { get; private set; }
    }
}