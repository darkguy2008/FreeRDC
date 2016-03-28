using System;
using System.Collections.Generic;

namespace FreeRDC.Network
{
    public static class RDCCommandExtensions
    {
        // http://stackoverflow.com/questions/28323772/convert-dictionarystring-object-to-class-and-subclass
        public static object GetObject(this IDictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null) continue;

                object value = kv.Value;
                if (value is IDictionary<string, object>)
                {
                    value = GetObject((IDictionary<string, object>)value, prop.PropertyType);
                }

                prop.SetValue(obj, value, null);
            }
            return obj;
        }
    }
}
