using System;
using FreeRDC.Services.Base;
using System.Net;
using FreeRDC.Network;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace FreeRDC.Services.Host
{
    public class HostService : BaseService
    {
        public CommandConnection Host;
        public string AssignedTag { get; set; }
        public string Password { get; set; }
        public Dictionary<string, IPEndPoint> OnlineClients { get; set; }

        private Thread _thScreenUpdate;
        private object _mutexClients = new object();
        private RDCScreenCapture _screencap = new RDCScreenCapture();

        public override void Init()
        {
            base.Init();
            Host = new CommandConnection();
            OnlineClients = new Dictionary<string, IPEndPoint>();
        }

        public override void Start()
        {
            base.Start();
            Host.OnCommandReceived += Host_OnCommandReceived;
            Console.WriteLine("Host listening at {0}:{1}", Address, Port);
            Host.Server(Address, Port);
        }

        private void Host_OnCommandReceived(IPEndPoint ep, CommandContainer cmd)
        {
            if (cmd.ID != AssignedTag)
                return;

            switch((ECommandType)cmd.Type)
            {
                case ECommandType.CLIENT_LOGIN:
                    var cmdLogin = Serializer.DeserializeAs<Commands.CLIENT_LOGIN>(cmd.Command);
                    if (cmdLogin.Password == Password)
                    {
                        lock (_mutexClients)
                            OnlineClients.Add(ep.ToString(), ep);
                        Host.SendCommand(ep, AssignedTag, new Commands.CLIENT_LOGIN_OK(), null);
                        if (_thScreenUpdate == null || !_thScreenUpdate.IsAlive)
                        {
                            _thScreenUpdate = new Thread(new ThreadStart(ThreadScreenUpdate));
                            _thScreenUpdate.Start();
                        }
                    }
                    break;
                case ECommandType.INTRODUCER:
                    var cmdIntroducer = Serializer.DeserializeAs<Commands.INTRODUCER>(cmd.Command);
                    break;
            }
        }

        private void ThreadScreenUpdate()
        {
            int count = 0;
            lock (_mutexClients)
                count = OnlineClients.Count;
            while (count > 0)
            {
                MemoryStream ms = _screencap.Capture3();
                lock (_mutexClients)
                    Parallel.ForEach(OnlineClients, (kvp) =>
                    {
                        // Host.SendCommand(kvp.Value, AssignedTag, new Commands.HOST_SCREENREFRESH() { Buffer = ms.ToArray() });
                    });
                Thread.Sleep(100);
                lock (_mutexClients)
                    count = OnlineClients.Count;
            }
        }

        public void Stop()
        {
            Host.Shutdown();
        }
    }
}
