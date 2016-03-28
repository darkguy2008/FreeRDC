using ENet;
using System;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandInterface
    {
        internal JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public virtual void OnCommandReceived(Event evt, RDCCommand cmd) { }
        public virtual void SendTimeout(Peer destination, RDCCommandChannel channel, RDCCommand cmd) { }

        public void SendCommand(Peer destination, RDCCommand cmd)
        {
            Packet p = new Packet();
            p.Initialize(Encoding.UTF8.GetBytes(_serializer.Serialize(cmd)), PacketFlags.Reliable);
            try
            {
                destination.Send((byte)cmd.Channel, p);
            }
            catch (InvalidOperationException)
            {
                // TODO: What to do here?
            }
            catch (ENetException)
            {
                SendTimeout(destination, cmd.Channel, cmd);
            }
        }
    }
}
