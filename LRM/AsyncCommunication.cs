using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRM
{
    public class AsyncCommunication
    {
        public Socket AsyncSocket { get; private set; }
        public bool LastOne { get; private set; }
        
        private ManualResetEvent PacketsReceived = new ManualResetEvent(false);
        private ManualResetEvent PacketsSend = new ManualResetEvent(false);
        private Action<AsyncCommunication> ConnectionLostCallback;
        private Action<string, AsyncCommunication> DataRedCallback;
        private Action<string, AsyncCommunication> SubscribeCallback;
        private Thread ReciveThread;
        private bool IdleState;
        
        public AsyncCommunication(Socket asyncSocket,
            Action<AsyncCommunication> connectionLostCallback,
            Action<string, AsyncCommunication> dataRedCallback,
            Action<string, AsyncCommunication> subscribeCallback)
        {
            AsyncSocket = asyncSocket;
            LastOne = true;
            DataRedCallback = dataRedCallback;
            SubscribeCallback = subscribeCallback;
            IdleState = true;
            ConnectionLostCallback = connectionLostCallback;
        }

        public void StartReciving()
        {
            ReciveThread = new Thread(Recive);
            ReciveThread.Start();
        }

        private void Recive()
        {
            try
            {
                while (LastOne)
                {
                    PacketsReceived.Reset();
                    StateObject state = new StateObject();
                    AsyncSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    PacketsReceived.WaitOne();
                }
            }
            catch (Exception ex)
            {
                AsyncSocket = null;
                if (ConnectionLostCallback != null)
                {
                    ConnectionLostCallback(this);
                }
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            PacketsReceived.Set();
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            int bytesRead = AsyncSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                string data = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);                
                state.ResponseBuilder.Append(data);

                if (bytesRead != StateObject.BufferSize)
                {
                    string allData = state.ResponseBuilder.ToString();

                    if (SubscribeCallback != null && IdleState && allData.Contains("INTRODUCE"))
                    {
                        SubscribeCallback(allData, this);
                        IdleState = false;
                        return;
                    }
                    
                    DataRedCallback(allData,this);
                }
                else
                {
                    AsyncSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        public void Send(String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            PacketsSend.Reset();
            StateObject state = new StateObject();
            AsyncSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), null);
            PacketsSend.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = AsyncSocket.EndSend(ar);
                PacketsSend.Set();
            }
            catch (Exception e)
            {
                AsyncSocket = null;
                Console.WriteLine(e.ToString());
                if (ConnectionLostCallback != null)
                {
                    ConnectionLostCallback(this);
                }
            }
        }
    }
}
