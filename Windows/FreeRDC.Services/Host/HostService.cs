using FreeRDC.Network;
using FreeRDC.Services.Base;
using FreeRDC.Services.Master;
using SharpRUDP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FreeRDC.Services.Host
{
    // TODO: UNIFY ALL INTO 1 CONNECTION
    // USE FUNCTION TO RETURN BOOL TRUE OR FALSE IF PACKET WAS PROCESSED
    // ADD CHAINS AS IN
    // BOOL KEEPPROCESSING = PROCESSHOSTPACKET(PKT); PROCESSCLIENTPACKET(PKT); PROCESSMASTERPACKET(PKT) IN THAT ORDER

    public class HostService : BaseService, ICommandService
    {
        public string AssignedTag { get; set; }
        public string Password { get; set; }

        public Dictionary<string, RUDPChannel> OnlineClients { get; set; }
        public CommandConnection HostConnection { get; set; }
        public MasterService Master { get; set; }

        private Thread _thScreenUpdate;
        private object _mutexClients = new object();
        private RDCScreenCapture _screencap = new RDCScreenCapture();

        public override void Init()
        {
            base.Init();
            OnlineClients = new Dictionary<string, RUDPChannel>();
        }

        public void ProcessCommand(RUDPChannel channel, CommandContainer cmd)
        {
            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.CLIENT_LOGIN:
                    var cmdLogin = Serializer.DeserializeAs<Commands.CLIENT_LOGIN>(cmd.Command);
                    //if (cmdLogin.Password == Password || true)
                    if (true)
                    {
                        lock (_mutexClients)
                            OnlineClients.Add(channel.Name, channel);
                        HostConnection.SendCommand(channel, Master.AssignedTag, new Commands.CLIENT_LOGIN_OK(), null);
                        if (_thScreenUpdate == null || !_thScreenUpdate.IsAlive)
                        {
                            _thScreenUpdate = new Thread(new ThreadStart(ThreadScreenUpdate));
                            _thScreenUpdate.Start();
                        }
                    }
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
                        HostConnection.SendCommand(kvp.Value, AssignedTag, new Commands.HOST_SCREENREFRESH() { Buffer = ms.ToArray() });
                    });
                Thread.Sleep(100);
                lock (_mutexClients)
                    count = OnlineClients.Count;
            }
        }

        public void Stop() { }

        public void Connected(RUDPChannel channel) { }

        public void Connection(RUDPChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
