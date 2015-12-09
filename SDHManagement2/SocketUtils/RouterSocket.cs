using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SDHManagement2.SocketUtils
{
    public class RouterSocket
    {
        private Thread personalThread;
        private TcpClient client;
        private List<byte[]> inputBuffer;
        private Socket socket;
        public string identifier { get; set; }
        public int port { get; set; }

        
        public RouterSocket(int _port, string name)
        {
            inputBuffer = new List<byte[]>();
            port = _port;
            identifier = name;
        }

        public void TurnOn()
        {
            //personalThread = new Thread(new ThreadStart(initConversation));
           // personalThread.Start();
           client = new TcpClient();
           initConversation();
        }

        private void initConversation()
        {
            
                socket = LocalSocektBuilder.Instance.getTcpSocket(port);



        }

        public Socket GetSocket()
        {
            return socket;
        }
    }
}
