using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud.SocketUtils;

namespace WireCloud
{

    public class Link
    {
        class StateObject
        {
            public Socket workSocket = null;
            public Socket sender = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        private LocalSocektBuilder socketBuilder;
        private int cloudServer;
        private int networkNodeServer;
        private string identificator;
       
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        public Link(int cloudServer, int networkNodeServer)
        {
            socketBuilder = LocalSocektBuilder.Instance;
            this.cloudServer = cloudServer;
            this.networkNodeServer = networkNodeServer;
            identificator = "[Link " + cloudServer + " : " + networkNodeServer +"]";
        }

        public void startListening()
        {
            logProggress("Starting listening");
            try
            {
                Socket listener = socketBuilder.getTcpSocket(cloudServer);
                while (true)
                {
                    listener.Listen(1);
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptIncomingConnection), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //Ten callback odpala sie w momencie podłączenia hosta
        private void AcceptIncomingConnection(IAsyncResult ar)
        {
            allDone.Set();
            Socket handler = ((Socket)ar.AsyncState).EndAccept(ar); // Tworzy Socket do obsługi przychodzącego
            StateObject state = new StateObject();
            
            logProggress("Connection accepted");
            
            state.workSocket = handler;
            // TODO sprawdzić czy nie będzie sytuacji w której spróbujemy stworzyć dwie wtyczki na tym samym porcie -> przy większej ilości kabli
            state.sender = socketBuilder.getTcpSocket();
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(StartReadingData), state); 
        }

        public void StartReadingData(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket; //odzyskanie handlera
            Socket sender = state.sender;
            
            int bytesRead = handler.EndReceive(ar);
            logProggress("Reading data finished");

            if (bytesRead > 0)
            {
                IPEndPoint endpoint = socketBuilder.getLocalEndpoint(networkNodeServer);
                sender.BeginConnect(endpoint, new AsyncCallback(ConnectToNextNode), sender); 
                connectDone.WaitOne();

                sendData(sender,state.buffer);
                sendDone.WaitOne();
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                
                
                
            }
            //TODO wprowadzić jakąś sekwencje końcową albo ograniczoną liczbębajtówktórą będziemy przesyłać  
            //Tu może wystąpić błąd związany ze zbyt dużm blokeim danych dlatego przyda się else który wywoła jeszcze raz StartReadingData
        }

        private void ConnectToNextNode(IAsyncResult ar)
        {
            try
            {
                Socket sender = (Socket)ar.AsyncState;
                sender.EndConnect(ar);
                logProggress("Next node Conected");
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void sendData(Socket sender, byte[] dataToSend)
        {
            logProggress("Sending data");
            sender.BeginSend(dataToSend, 0, dataToSend.Length, 0,
                new AsyncCallback(SendData), sender);
        }

        private void SendData(IAsyncResult ar)
        {
            try {
                Socket client = (Socket) ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                logProggress("Sent " + bytesSent + " bytes to server.");
                sendDone.Set();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        private void  logProggress(string message){
            Console.WriteLine(identificator + " " + message);
        }

    }
}
