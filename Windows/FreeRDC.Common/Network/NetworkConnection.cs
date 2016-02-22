using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace FreeRDC.Common.Network
{
    public class NetworkConnection
    {

        public bool IsAlive { get; set; }
        public Thread ClientThread { get; set; }
        public NetworkStream ClientStream { get; set; }
        public TcpClient Connection { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        private BinaryFormatter BinFormat = new BinaryFormatter();
        private delegate void DataRecvEventDelegate(RDCCommandStruct data);
        private event DataRecvEventDelegate OnDataReceived;

        public virtual void Start()
        {
            IsAlive = true;
            ClientStream = Connection.GetStream();
            DataReader = new BinaryReader(ClientStream);
            DataWriter = new BinaryWriter(ClientStream);
            OnDataReceived += new DataRecvEventDelegate(NetworkConnection_OnDataReceived);
            ClientThread = new Thread(new ThreadStart(ListenThread));
            ClientThread.IsBackground = true;
            ClientThread.Start();
        }

        private void NetworkConnection_OnDataReceived(RDCCommandStruct data)
        {
            ProcessCommand(data);
        }

        public virtual void Stop()
        { 
            IsAlive = false;
        }

        public void ListenThread()
        {
            while (IsAlive)
            {
                if (ClientStream.DataAvailable)
                    OnDataReceived.Invoke((RDCCommandStruct)BinFormat.Deserialize(ClientStream)); // TODO: Fast-clicking = Exception?                    
                Thread.Sleep(1);
            }
        }

        public void SendCommand(BinaryWriter bw, RDCCommandType cmd, params object[] arguments)
        {
            MemoryStream ms = new MemoryStream();
            RDCCommandStruct data = new RDCCommandStruct() { Command = cmd };
            if (arguments != null)
                data.Payload = arguments;
            BinFormat.Serialize(ms, data);
            Debug.WriteLine("Sending " + ms.Length + " bytes");
            bw.Write(ms.ToArray());
        }
        public void SendCommand(RDCCommandType cmd, params object[] arguments)
        {
            SendCommand(DataWriter, cmd, arguments);
        }

        private void ProcessCommand(RDCCommandStruct command)
        {
            object[] data = (object[])command.Payload;
            ProcessCommand(command.Command, data);
        }

        public virtual void ProcessCommand(RDCCommandType type, object[] data)
        {
            Debug.WriteLine("COMMAND: " + type);
            Console.WriteLine("COMMAND: " + type);
        }

    }
}
