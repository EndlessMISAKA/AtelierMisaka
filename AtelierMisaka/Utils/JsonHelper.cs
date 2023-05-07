using AtelierMisaka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka.Utils
{
    internal class JsonHelper
    {
        internal static JObject GetJObject(string json)
        {
            return JObject.Parse(json);
        }

        internal static string ConverToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        internal static T ToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
