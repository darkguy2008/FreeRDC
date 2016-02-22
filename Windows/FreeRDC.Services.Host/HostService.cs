using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Threading;
using System.Net.Sockets;
using FreeRDC.Common.Hardware;
using System.IO;

namespace FreeRDC.Services.Host
{
    public class HostService : NetworkConnection
    {
        public string MasterHostname { get; set; }
        public string SlotID { get; set; }
        public string Password { get; set; }

        public delegate void StringDataEventDelegate(string arg);
        public event StringDataEventDelegate OnConnected;

        private Thread thStream;
        private volatile bool isStreaming = false;
        private RDCScreenCapture screencap = new RDCScreenCapture();
        long oldLen = 0; // Basic method for checking if to send a new image

        public void Start(string masterHostname)
        {
            MasterHostname = masterHostname;
            string fingerprint = HWID.GenerateFingerprint();

            Connection = new TcpClient();
            Connection.Connect(MasterHostname, 80);
            base.Start();

            SendCommand(RDCCommandType.IAMHOST, fingerprint);
        }

        public override void ProcessCommand(RDCCommandType type, object[] data)
        {
            base.ProcessCommand(type, data);
            switch (type)
            {
                case RDCCommandType.IAMHOST_OK:
                    SlotID = (string)data[0];
                    if (OnConnected != null)
                        OnConnected.Invoke(SlotID);
                    break;

                case RDCCommandType.CLIENT_CONNECT:
                    SlotID = (string)data[0];
                    int cID = (int)data[1];
                    string pw = (string)data[2];
                    if (pw == Password)
                        SendCommand(RDCCommandType.CLIENT_CONNECT_OK, SlotID, cID);
                    else
                        SendCommand(RDCCommandType.CLIENT_CONNECT_FAIL, SlotID, cID);
                    break;

                case RDCCommandType.STREAM_START:
                    if (thStream != null && !thStream.IsAlive)
                        thStream = null;
                    isStreaming = true;
                    thStream = new Thread(new ThreadStart(ThreadStream));
                    thStream.IsBackground = true;
                    thStream.Start();
                    break;
            }
        }

        public void ThreadStream()
        {
            // TODO: 1st send full image then dirty updates            
            while (isStreaming)
            {
                MemoryStream ms = screencap.Capture3();
                if (oldLen != ms.Length)
                    SendCommand(RDCCommandType.REFRESH_FULL, SlotID, ms.ToArray());
                oldLen = ms.Length;
            }
        }
    }
}
