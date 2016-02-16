using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace FreeRDC.Services.Host
{
    public class FreeRDCMaster : BaseNetwork
    {
        public Thread Thread { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public bool IsAlive { get; set; }
        private ManualResetEvent sendDone; 

        public string MasterServerHostname { get; set; }

        public void Start()
        {
            IsAlive = true;
            IPHostEntry ipHostInfo = Dns.Resolve(MasterServerHostname);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Client = new TcpClient();
            Client.Connect(ipAddress, 80);
            ClientStream = Client.GetStream();

            Thread = new Thread(new ThreadStart(MasterThread));
            Thread.Start();
        }

        public void MasterThread()
        {
            DataReader = new BinaryReader(ClientStream);
            DataWriter = new BinaryWriter(ClientStream);
            SendCommand(DataWriter, "AUTHREQID", null);
            while (IsAlive)
                if (ClientStream.DataAvailable) { }
                    //ProcessCommand((CommandStruct)binFmt.Deserialize(ClientStream));
        }
    }
}
