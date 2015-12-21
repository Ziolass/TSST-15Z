using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WireCloud.CloudLogic
{
    public class NetworkNodeSender : Port
    {
        public NetworkNodeSender(int port)
            : base(port)
        {

        }
        
        public void SetUpClinet()
        {
            try
            {
                SetSocket();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot perform SetSocket()");
            }
        }

        public void SendContent(string json)
        {
            try
            {
                SetSocket();
                ActionSocket.Connect(ActionEndPoint);
                
                byte[] msg = Encoding.ASCII.GetBytes(json);
                
                ActionSocket.Send(msg);
                ActionSocket.Shutdown(SocketShutdown.Both);
                ActionSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }


        }
        public bool Equals(NetworkNodeSender networkNodeSender)
        {
            if (this.PortNumber == networkNodeSender.PortNumber)
                return true;
            else return false;
        }
    }
}
