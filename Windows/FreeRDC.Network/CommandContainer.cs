namespace FreeRDC.Network
{
    public class CommandContainer
    {
        public string ID { get; set; }
        public byte Type { get; set; }
        public byte[] Command { get; set; }
    }
}
