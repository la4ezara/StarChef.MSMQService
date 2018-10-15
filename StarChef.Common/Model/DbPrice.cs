using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class DbPrice : IEqualityComparer<DbPrice>
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("group_id")]
        public int GroupId { get; set; }

        private decimal _price = 0m;

        [Description("product_price")]
        public decimal Price {
            get {
                return _price;
            }
            set {
                _price = decimal.Round(value, 8);
            }
        }

        public bool Equals(DbPrice x, DbPrice y)
        {
            if (x.ProductId.Equals(y.ProductId)
                && x.GroupId.Equals(y.GroupId))
            {
                if (!x.Price.Equals(y.Price)) {
                    var ss = decimal.Subtract(x.Price, y.Price);
                    if (Math.Abs(ss) > 0.00000001m)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public int GetHashCode(DbPrice obj)
        {
            return obj.ProductId.GetHashCode() ^ obj.GroupId.GetHashCode() ^ obj.Price.GetHashCode();
        }
    }
}
