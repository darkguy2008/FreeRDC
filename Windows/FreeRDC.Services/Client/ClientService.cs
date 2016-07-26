using FreeRDC.Network;
using FreeRDC.Services.Base;
using SharpRUDP;
using System;

namespace FreeRDC.Services.Client
{
    public class ClientService : BaseService, ICommandService
    {
        public CommandConnection ClientConnection { get; set; }
        public FreeRDCService Service { get; set; }
        public string Password { get; set; }
        public string HostTag { get; set; }
        public int HostPort { get; set; }
        public string HostAddress { get; set; }

        public delegate void dlgVoid();
        public event dlgVoid OnLoggedIn;

        private RUDPChannel _channel;

        public void Connect(string hostTag)
        {
            HostTag = hostTag;
            Service.RegisterCommandService(this);
            Service.ConnectToHost(hostTag);
        }

        public void Login(string pwd)
        {
            Password = pwd;
            ClientConnection.Connect(HostAddress, HostPort, HostTag);
        }

        private void ClientConnection_OnConnected(RUDPChannel channel)
        {
            Connected(channel);
        }

        public void ProcessCommand(RUDPChannel channel, CommandContainer cmd)
        {
            if (cmd.Tag != HostTag)
                return;

            // TODO: Receive & Process image
            //       Rewrite app in easier format to handle

            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.CLIENT_LOGIN_OK:
                    OnLoggedIn?.Invoke();
                    break;
            }
        }

        public void Connected(RUDPChannel channel)
        {
            _channel = channel;
            ClientConnection.SendCommand(_channel, HostTag, new Commands.CLIENT_LOGIN() { Password = Password }, () =>
            {
                RUDPConnection.Debug("Logging to {0} with password {1}...", HostAddress + ":" + HostPort, Password);
            });
        }

        public void Connection(RUDPChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}