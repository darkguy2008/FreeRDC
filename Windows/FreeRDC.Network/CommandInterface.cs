using ENet;
using System;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandInterface
    {
        internal JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public virtual void OnCommandReceived(Event evt, RDCCommand cmd) {
            //string debug = string.Format("[{0}] [{1}:{2}] <- {3}: {4}", this.GetType().Name, evt.ChannelID, evt.Peer.GetRemoteAddress(), cmd.Command, cmd.StringData);
            //Console.WriteLine(debug);
            //Debug.WriteLine(debug);
        }

        public void SendCommand(Peer destination, RDCCommandChannel channel, RDCCommandType cmd, string stringData, byte[] data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            //string debug = string.Format("[{0}] [{1}:{2}] -> {3}: {4}", this.GetType().Name, (byte)channel, destination.GetRemoteAddress(), cmd, stringData);
            //Console.WriteLine(debug);
            //Debug.WriteLine(debug);
            Packet packet = new Packet();
            packet.Initialize(Encoding.UTF8.GetBytes(_serializer.Serialize(new RDCCommand()
            {
                Command = cmd,
                StringData = stringData,
                ByteData = data
            })), flags);
            try
            {
                destination.Send((byte)channel, packet);
            }
            catch(InvalidOperationException)
            {

            }
            catch(ENetException)
            {
                SendTimeout(destination, channel, cmd, stringData, data, flags);
            }
        }

        public virtual void SendTimeout(Peer destination, RDCCommandChannel channel, RDCCommandType cmd, string stringData, byte[] data, PacketFlags flags)
        {
        }
    }
}
