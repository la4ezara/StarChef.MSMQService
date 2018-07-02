using System;

namespace Fourth.Import.Model
{
    public class LookupValue : IComparable<LookupValue>
    {
        public string Lookup { get; set; }
        public int ReplacementId { get; set; }

        public int CompareTo(LookupValue other)
        {
            return Lookup.CompareTo(other.Lookup);
        }
    }
}