using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RazorSummerVacationFlights.Helpers
{
    public static class JsonHelper
    {
        public static T DeserializeJSon<T>(string jsonString)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var obj = (T)ser.ReadObject(stream);
            return obj;
        }
    }
}
