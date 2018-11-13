using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class DbPrice : IEqualityComparer<DbPrice>
    {
        public readonly int Rounding = 6;
        public readonly decimal Delta;

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
                _price = decimal.Round(value, Rounding);
            }
        }

        public bool Equals(DbPrice x, DbPrice y)
        {
            if (x.ProductId.Equals(y.ProductId)
                && x.GroupId.Equals(y.GroupId))
            {
                if (!x.Price.Equals(y.Price)) {
                    var ss = decimal.Subtract(x.Price, y.Price);
                    if (Math.Abs(ss) > Delta)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            var x = this;
            DbPrice y = obj as DbPrice;
            if (y != null) {
                if (x.ProductId.Equals(y.ProductId)
                    && x.GroupId.Equals(y.GroupId))
                {
                    if (!x.Price.Equals(y.Price))
                    {
                        var ss = decimal.Subtract(x.Price, y.Price);
                        if (Math.Abs(ss) > Delta)
                        {
                            return false;
                        }
                    }

                    return true;
                }
                return false;
            }
            return false;
        }

        public int GetHashCode(DbPrice obj)
        {
            return obj.ProductId.GetHashCode() ^ obj.GroupId.GetHashCode() ^ obj.Price.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ProductId.GetHashCode() ^ GroupId.GetHashCode() ^ Price.GetHashCode();
        }



        public DbPrice() {
            Delta = 25 * (decimal)Math.Pow(10, -1 * Rounding);
        }
    }
}
