using FreeRDC.Network;
using FreeRDC.Services.Base;
using System;
using System.Net;

namespace FreeRDC.Services.Master
{
    public class MasterService : BaseService
    {
        public CommandConnection Master;
        public string AssignedTag { get; set; }
        public string Fingerprint { get; set; }

        public delegate void dlgConnected(IPEndPoint ep);
        public delegate void dlgAuthenticated(string tag, string address);
        public delegate void dlgIntroducer(Commands.INTRODUCER introducer);
        public event dlgConnected OnConnected;
        public event dlgAuthenticated OnAuthenticated;
        public event dlgIntroducer OnIntroducerPacket;

        public override void Init()
        {
            base.Init();
            Master = new CommandConnection();
        }

        public override void Start()
        {
            base.Start();
            Master.OnConnected += MasterConnection_OnConnected;
            Master.OnCommandReceived += MasterConnection_OnCommandReceived;
            Master.Client(Address, Port);
        }

        private void MasterConnection_OnConnected(IPEndPoint ep)
        {
            Master.RemoteEndPoint = ep;
            Master.SendCommand(Master.RemoteEndPoint, null, new Commands.AUTH() { Fingerprint = Fingerprint }, () =>
            {
                Console.WriteLine("Identifying with fingerprint {0}...", Fingerprint);
                OnConnected?.Invoke(ep);
            });
        }

        private void MasterConnection_OnCommandReceived(IPEndPoint ep, CommandContainer cmd)
        {
            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmdAuth = Serializer.DeserializeAs<Commands.AUTH_OK>(cmd.Command);
                    if (cmd.ID == "MASTER")
                    {
                        AssignedTag = cmdAuth.AssignedID;
                        Console.WriteLine("Assigned tag: " + cmdAuth.AssignedID);
                        Console.WriteLine("Outside address: " + cmdAuth.EndpointAddress);
                        OnAuthenticated?.Invoke(cmdAuth.AssignedID, cmdAuth.EndpointAddress);
                    }
                    break;
                case ECommandType.INTRODUCER:
                    var cmdIntroducer = Serializer.DeserializeAs<Commands.INTRODUCER>(cmd.Command);
                    OnIntroducerPacket?.Invoke(cmdIntroducer);
                    break;
            }
        }
    }
}
