using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o;
            tempData.TryGetValue(key, out o);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }

        public static T Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o = null;
            if (tempData.ContainsKey(key))
                o = tempData.Peek(key);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }

        public static void Put<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string o = session.GetString(key);
            if (string.IsNullOrEmpty(o))
                return default;
            return JsonConvert.DeserializeObject<T>(o);
        }

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
            T result = default;
            try
            {
                result = JsonConvert.DeserializeObject<T>(obj);
            }
            catch (Exception)
            {
                return default;
            }
            return result;
        }
    }

}
