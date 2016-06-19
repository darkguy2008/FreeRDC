using FreeRDC.Common;
using FreeRDC.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FreeRDC.Services.Client
{
    public class ClientService
    {
        public static CommandSerializer Serializer = new CommandSerializer();

        public string AssignedTag { get; set; }
        public string Fingerprint { get; set; }
        public List<HostConnection> HostConnections { get; set; }

        public delegate void dlgConnectionEvent(HostConnection connection);
        public delegate void dlgMasterLoggedIn(string assignedId);
        public event dlgConnectionEvent OnNewConnection;
        public event dlgMasterLoggedIn OnMasterLoggedIn;

        private CommandConnection _master;
        private static CommandSerializer _cs = new CommandSerializer();

        public ClientService()
        {
            HostConnections = new List<HostConnection>();
        }

        public void ConnectToMaster(string address, int port)
        {
            //Fingerprint = HWID.GenerateFingerprint();
            Fingerprint = "123";
            _master = new CommandConnection();
            _master.OnConnected += OnConnected;
            _master.OnCommandReceived += OnCommandReceived;
            _master.Client(address, port);
        }

        public void ConnectToHost(string hostTag)
        {
            _master.SendCommand(_master.RemoteEndPoint, hostTag, new Commands.CLIENT_CONNECTIONREQUEST());
        }

        private void OnConnected(IPEndPoint ep)
        {
            _master.RemoteEndPoint = ep;
            _master.SendCommand(_master.RemoteEndPoint, null, new Commands.AUTH() { AuthType = (int)Commands.AUTH.AuthTypes.Client, Fingerprint = Fingerprint }, () =>
            {
                Console.WriteLine("Identifying CLIENT with fingerprint {0}...", Fingerprint);
            });
        }

        private void OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmdAuth = _cs.DeserializeAs<Commands.AUTH_OK>(command.Command);
                    if (command.ID == "MASTER")
                    {
                        AssignedTag = cmdAuth.AssignedID;
                        Console.WriteLine("CLIENT Assigned tag: " + cmdAuth.AssignedID);
                        Console.WriteLine("CLIENT Endpoint address: " + cmdAuth.EndpointAddress);
                        OnMasterLoggedIn?.Invoke(cmdAuth.AssignedID);
                    }
                    break;
                case ECommandType.INTRODUCER:
                    var cmdIntroducer = _cs.DeserializeAs<Commands.INTRODUCER>(command.Command);
                    HostConnection hostConnection = null;
                    lock(HostConnections)
                    {
                        HostConnections.Add(new HostConnection(cmdIntroducer.RemoteEndPointAddress.ToEndPoint()) { ID = command.ID });
                        hostConnection = HostConnections.Last();
                    }
                    OnNewConnection?.Invoke(hostConnection);
                    break;
            }
        }
    }
}
