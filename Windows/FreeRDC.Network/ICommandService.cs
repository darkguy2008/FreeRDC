using SharpRUDP;

namespace FreeRDC.Network
{
    public interface ICommandService
    {
        void ProcessCommand(RUDPChannel channel, CommandContainer cmd);
        void Connected(RUDPChannel channel);
        void Connection(RUDPChannel channel);
    }
}
