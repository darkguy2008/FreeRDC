namespace FreeRDC.Network
{
    public class CommandContainer
    {
        public string TagFrom { get; set; }
        public string TagTo { get; set; }
        public byte Type { get; set; }
        public byte[] Command { get; set; }
    }
}
