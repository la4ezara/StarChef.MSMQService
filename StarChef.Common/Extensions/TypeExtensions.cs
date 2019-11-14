using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourth.StarChef.Invariables;
using Newtonsoft.Json;

namespace StarChef.Common.Extensions
{
    public static class TypeExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj);
        }

        public static string ToJson(this UpdateMessage obj)
        {
            UpdateMessage cloneMsg = new UpdateMessage(obj);
            cloneMsg.DSN = string.Empty;

            return obj == null ? string.Empty : JsonConvert.SerializeObject(cloneMsg);
        }
    }
}
