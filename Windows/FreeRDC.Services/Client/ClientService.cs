using System;
using FreeRDC.Services.Master;

namespace FreeRDC.Services
{
    public class ClientService
    {
        public MasterService Master { get; set; }
        public string Password { get; set; }

        public void Connect(string hostTag, string password)
        {
            Password = password;
            Master.ConnectToHost(hostTag);
        }
    }
}
