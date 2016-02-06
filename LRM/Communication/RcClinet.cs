using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRM.Communication
{
    class RcClinet : LocalPort
    {
        private static ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private RcAsyncComm Async;
        private Action<string, RcAsyncComm> DataRedCallback;
        private EventWaitHandle WaitHandler = new AutoResetEvent(false);
        private Thread ReciverThread;
        private object ConnectLock = new object();
        private int Port;


        public RcClinet(int port, Action<string, RcAsyncComm> callback)
            : base(port)
        {
            this.Port = port;
            DataRedCallback = callback;
        }

        public void ConnectToRc()
        {
            lock (this)
            {
                if (Async != null)
                {
                    return;
                }
                ConnectDone.Reset();
                StateObject state = new StateObject();
                Console.WriteLine("--------------------------------------RC CLIENT BEGIN CONNECT");
                ActionSocket.BeginConnect(Endpoint, new AsyncCallback(ConnectCallback), null);
                ConnectDone.WaitOne();

            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("--------------------------------------RC START END CONNECT");
                ActionSocket.EndConnect(ar);
                Console.WriteLine("--------------------------------------RC END END CONNECT");
                ConnectDone.Set();
                Async = new RcAsyncComm(ActionSocket, null, DataRedCallback, null);


                Async.StartReciving();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendToRc(string msg)
        {
            lock (Async)
            {
                Async.Send(msg);
            }
        }
    }
}
