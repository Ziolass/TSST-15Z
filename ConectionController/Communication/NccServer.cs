using LRM;
using System;

namespace Cc.Communication
{
    internal class NccServer : LocalPort
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