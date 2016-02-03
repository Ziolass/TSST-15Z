using LRM;
using NetworkNode.LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConectionController.Communication;
using Newtonsoft.Json;

namespace Cc.Communication
{
    class NccServer : LocalPort
    {
        private Action<string, AsyncCommunication> DataRedCallback;
        private AsyncCommunication NccCommunication;
        public NccServer(int port, Action<string, AsyncCommunication> dataRedCallback)
            : base(port)
        {
            DataRedCallback = dataRedCallback;
        }

        public void Start()
        {
            try
            {
                ActionSocket.Bind(Endpoint);
                ActionSocket.Listen(100);


                Console.WriteLine("Waiting for a connection...");
                ActionSocket.BeginAccept(
                       new AsyncCallback(AcceptCallback),
                       null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            NccCommunication = new AsyncCommunication(ActionSocket.EndAccept(ar),
                null,
                DataRedCallback,
                null);

            NccCommunication.StartReciving();
        }

        public void Send(string data)
        {
            NccCommunication.Send(data);
        }

    }
}
