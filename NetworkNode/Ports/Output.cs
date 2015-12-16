using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TsstSdh.SocketUtils;

namespace NetworkNode.Ports
{
    public class Output : IDisposable
    {
        private LocalSocektBuilder socketBuilder;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private int outputPort;

        public bool Active { get; set; }
        public Output(int port)
        {
            socketBuilder = LocalSocektBuilder.Instance;
            outputPort = port;
        }

        public int Port
        {
            get
            {
                return outputPort;
            }
        }

        public void sendData(byte[] dataToSend)
        {
            
            Socket sender = socketBuilder.getTcpSocket();
            sender.DontFragment = true;
            IPEndPoint endpoint = socketBuilder.getLocalEndpoint(outputPort);
            try
            {
                sender.BeginConnect(endpoint, new AsyncCallback(ConnectToNextNode), sender);
                connectDone.WaitOne();

                sendData(sender, dataToSend);
                sendDone.WaitOne();
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void ConnectToNextNode(IAsyncResult ar)
        {
            try
            {
                Socket sender = (Socket)ar.AsyncState;
                sender.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void sendData(Socket sender, byte[] dataToSend)
        {
            sender.BeginSend(dataToSend, 0, dataToSend.Length, 0,
                new AsyncCallback(SendData), sender);
        }

        private void SendData(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
