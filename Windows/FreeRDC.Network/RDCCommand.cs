namespace FreeRDC.Network
{
    public class RDCCommand
    {
        public RDCCommandType Command { get; set; }
        public string StringData { get; set; }
        public byte[] ByteData { get; set; }
    }
}
