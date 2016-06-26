using FreeRDC.Network;

namespace FreeRDC.Services.Base
{
    public class BaseService
    {
        public static CommandSerializer Serializer = new CommandSerializer();

        public int Port { get; set; }
        public string Address { get; set; }

        public virtual void Init() { }
        public virtual void Start() { }
    }
}
