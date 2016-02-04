using System;
using System.Net.Sockets;
using System.Text;

namespace Cc.Communication
{
    public class NetworkNodeSender : Port
    {
        private int BufferSize;

        public NetworkNodeSender(int port, int bufferSize)
            : base(port)
        {
            BufferSize = bufferSize;
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

        public void SendContent(string json, Action<string> callback)
        {
            byte[] bytes = new byte[BufferSize];
            try
            {
                SetSocket();
                ActionSocket.Connect(ActionEndPoint);

                byte[] msg = Encoding.ASCII.GetBytes(json);

                ActionSocket.Send(msg);

                int bytesRec = ActionSocket.Receive(bytes);

                string incomingData = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                callback(incomingData);
                //UWAGA zamykam mogę nie zamykać
                ActionSocket.Shutdown(SocketShutdown.Both);
                ActionSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", json);
            }
        }
    }
}