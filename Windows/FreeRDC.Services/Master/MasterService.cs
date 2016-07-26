using FreeRDC.Common.Hardware;
using FreeRDC.Network;
using FreeRDC.Services.Base;
using SharpRUDP;
using System;

namespace FreeRDC.Services.Master
{
    public class MasterService : BaseService, ICommandService
    {
        public CommandConnection Connection { get; set; }
        public string AssignedTag { get; set; }
        public string Fingerprint { get; set; }

        public delegate void dlgAuthenticated(string tag, string address);
        public delegate void dlgIncomingConnection(Commands.INTRODUCER cmdIntroducer);
        public delegate void dlgHostConnection(CommandContainer cmd, Commands.INTRODUCER introducer);
        public event dlgAuthenticated OnAuthenticated;
        public event dlgHostConnection OnHostConnected;
        public event dlgIncomingConnection OnIncomingConnection;

        public override void Init()
        {
            Fingerprint = HWID.GenerateFingerprint();
        }

        public void ProcessCommand(RUDPChannel channel, CommandContainer cmd)
        {
            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmdAuth = CommandConnection.Serializer.DeserializeAs<Commands.AUTH_OK>(cmd.Command);
                    if (cmd.Tag == "MASTER")
                    {
                        AssignedTag = cmdAuth.AssignedID;
                        Console.WriteLine("Assigned tag: " + cmdAuth.AssignedID);
                        Console.WriteLine("Outside address: " + cmdAuth.EndpointAddress);
                        OnAuthenticated?.Invoke(cmdAuth.AssignedID, cmdAuth.EndpointAddress);
                    }
                    break;
                case ECommandType.INTRODUCER:
                    var cmdIntroducer = Serializer.DeserializeAs<Commands.INTRODUCER>(cmd.Command);
                    // TODO: Holepunch here
                    if (cmdIntroducer.IncomingConnection)
                        OnIncomingConnection?.Invoke(cmdIntroducer);
                    else
                        OnHostConnected?.Invoke(cmd, cmdIntroducer);
                    break;
            }
        }

        public void Connected(RUDPChannel channel) { }

        void ICommandService.Connection(RUDPChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
