using LRM;
using System;
using System.Threading;

namespace Cc.Communication
{
    internal class NccServer : LocalPort
    {
        private Action<string, AsyncCommunication> DataRedCallback;
        private AsyncCommunication NccCommunication;
        private ManualResetEvent PacketsReceived = new ManualResetEvent(false);

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

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    PacketsReceived.Reset();
                    ActionSocket.BeginAccept(
                           new AsyncCallback(AcceptCallback),
                           null);
                    PacketsReceived.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            PacketsReceived.Set();
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