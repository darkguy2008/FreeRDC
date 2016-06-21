using FreeRDC.Common;
using FreeRDC.Network;
using FreeRDC.Services.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace FreeRDC.Services
{
    public class HostService
    {
        public static CommandSerializer Serializer = new CommandSerializer();

        public string AssignedID { get; set; }
        public string Fingerprint { get; set; }
        public string PasswordToken { get; set; }
        public string PasswordGlobal { get; set; }
        public IPEndPoint OutsideEndpoint { get; set; }
        public List<ClientConnection> ClientConnections { get; set; }

        public delegate void dlgConnectionEvent(ClientConnection connection);
        public delegate void dlgMasterLoggedIn(string assignedId);
        public event dlgConnectionEvent OnNewClientConnection;
        public event dlgMasterLoggedIn OnMasterLoggedIn;

        private CommandConnection _master;

        public HostService()
        {
            ClientConnections = new List<ClientConnection>();
        }
        
        public void ConnectToMaster(string address, int port)
        {
            //Fingerprint = HWID.GenerateFingerprint();
            Fingerprint = "123";
            PasswordToken = "123";
            _master = new CommandConnection();
            _master.OnConnected += OnConnected;
            _master.OnCommandReceived += OnCommandReceived;
            _master.Client(address, port);
        }

        public object GetHostInfoCommand()
        {
            return new Commands.HOST_INFO()
            {
                ScreenWidth = Screen.PrimaryScreen.Bounds.Width,
                ScreenHeight = Screen.PrimaryScreen.Bounds.Height
            };
        }

        private void OnConnected(IPEndPoint ep)
        {
            _master.RemoteEndPoint = ep;
            _master.SendCommand(_master.RemoteEndPoint, null, new Commands.AUTH() { AuthType = (int)Commands.AUTH.AuthTypes.Host, Fingerprint = Fingerprint }, () =>
            {
                Console.WriteLine("Identifying HOST with fingerprint {0}...", Fingerprint);
            });
        }

        private void OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmdAuth = Serializer.DeserializeAs<Commands.AUTH_OK>(command.Command);
                    if (command.ID == "MASTER")
                    {
                        AssignedID = cmdAuth.AssignedID;
                        OutsideEndpoint = new IPEndPoint(IPAddress.Parse(cmdAuth.EndpointAddress.Split(':')[0]), int.Parse(cmdAuth.EndpointAddress.Split(':')[1]));
                        Console.WriteLine("HOST Assigned ID: " + cmdAuth.AssignedID);
                        Console.WriteLine("HOST Endpoint address: " + cmdAuth.EndpointAddress);
                        OnMasterLoggedIn?.Invoke(cmdAuth.AssignedID);
                    }
                    break;
                case ECommandType.INTRODUCER:
                    var cmdIntroducer = Serializer.DeserializeAs<Commands.INTRODUCER>(command.Command);
                    ClientConnection clientConnection = null;
                    lock (ClientConnections)
                    {
                        ClientConnections.Add(new ClientConnection(cmdIntroducer.RemoteEndPointAddress.ToEndPoint(), OutsideEndpoint) { HostSvc = this, ClientID = command.ID, HostID = AssignedID });
                        clientConnection = ClientConnections.Last();
                        clientConnection.Listen();
                    }
                    OnNewClientConnection?.Invoke(clientConnection);
                    break;
            }
        }

        public void Shutdown()
        {
            foreach (ClientConnection cn in ClientConnections)
                cn.Shutdown();
            _master.Shutdown();
        }
    }
}
