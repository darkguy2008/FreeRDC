using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace FreeRDC.Master
{
    public class RDCMasterClientConnection : RDCBaseNetwork
    {
        public Thread Thread { get; set; }
        public TcpClient Client { get; set; }
        public RDCMasterService Parent { get; set; }
        public TcpListener Listener { get; set; }
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public bool IsAlive { get; set; }
        
        public void Start()
        {
            IsAlive = true;
            Thread = new Thread(new ThreadStart(ClientThread));
            Thread.Start();
        }

        public void ClientThread()
        {
            DataReader = new BinaryReader(ClientStream);
            DataWriter = new BinaryWriter(ClientStream);
            while (IsAlive)
                if (ClientStream.DataAvailable)
                    ProcessCommand((RDCCommandStruct)binFmt.Deserialize(ClientStream));
        }

        public void ProcessCommand(RDCCommandStruct cmdData)
        {
            Console.WriteLine("Command: " + cmdData.Command);
            switch (cmdData.Command)
            {
                case RDCCommandType.AUTH_REQUEST_ID:
                    string newID = "LALALA";
                    SendCommand(DataWriter, RDCCommandType.AUTH_ASSIGNED_ID, newID);
                    break;

                default:
                    break;
            }
        }
    }
}
