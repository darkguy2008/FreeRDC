using System;
using FreeRDC.Services.Base;
using System.Net;

namespace FreeRDC.Services.Host
{
    public class HostService : BaseService
    {
        public string AssignedTag { get; set; }

        public override void Start()
        {
            base.Start();

        }

        public void Stop()
        {
        }
    }
}
