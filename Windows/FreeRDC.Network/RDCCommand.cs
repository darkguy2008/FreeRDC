using System.Collections.Generic;

namespace FreeRDC.Network
{
    public class RDCCommand
    {
        public string SourceID { get; set; }
        public string DestinationID { get; set; }
        public RDCCommandType Command { get; set; }
        public object Data { get; set; }
        public byte[] Buffer { get; set; }
        
        public T CastDataAs<T>()
        {
            IDictionary<string, object> dict = (IDictionary<string, object>)Data;
            return (T)RDCCommandExtensions.GetObject(dict, typeof(T));
        }
    }
}
