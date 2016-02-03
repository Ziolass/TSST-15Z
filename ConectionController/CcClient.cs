using LRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConectionController
{
    class CcClient : LocalPort
    {
         private static ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private AsyncCommunication Async;
        private Action<string, AsyncCommunication> DataRedCallback;
        private Thread ReciverThread;
        private object ConnectLock = new object();

        public CcClient(int port, Action<string, AsyncCommunication> callback)
            : base(port)
        {
            DataRedCallback = callback;
        }

        public void ConnectToCc()
        {
            lock (ConnectLock)
            {
                if (Async != null)
                {
                    return;
                }
                ConnectDone.Reset();
                StateObject state = new StateObject();
                ActionSocket.BeginConnect(Endpoint, new AsyncCallback(ConnectCallback), null);
                ConnectDone.WaitOne();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                ActionSocket.EndConnect(ar);
                ConnectDone.Set();
                Async = new AsyncCommunication(ActionSocket, null, DataRedCallback, null);

                ReciverThread = new Thread(new ThreadStart(Async.StartReciving));
                ReciverThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendToCc(string msg)
        {
            lock (ConnectLock)
            {
                Async.Send(msg);
            }
        }
    }
}
