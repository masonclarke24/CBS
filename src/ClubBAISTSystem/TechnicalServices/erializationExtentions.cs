using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechnicalServices
{
    public static class SerializationExtentions
    {
        public static string SerializeObject(this object obj)
        {
            if (obj is null)
                return null;
            return JsonConvert.SerializeObject(obj);
        }

        public static T DeserilizeObject<T>(this string obj)
        {
            if (string.IsNullOrWhiteSpace(obj))
                return default;
            T result;
            try
            {
                result = JsonConvert.DeserializeObject<T>(obj);
            }
            catch (Exception ex)
            {
                return default;
            }
            return result;
        }
    }

}
