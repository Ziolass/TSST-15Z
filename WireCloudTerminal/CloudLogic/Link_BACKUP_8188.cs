<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud.SocketUtils;

namespace WireCloud
{
    public delegate void LinkStateChangedHandler();
    public class Link
    {
        class StateObject
        {
            public Socket workSocket = null;
            public Socket sender = null;
            public const int BufferSize = 10024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        private LocalSocektBuilder socketBuilder;
        public int Source { get; private set; }
        public int Destination { get; private set; }
        private bool running;
        private Socket listener;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        public event LinkStateChangedHandler LinkActive;

        private bool active;
        public bool IsLinkActive
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                if (LinkActive != null)
                {
                    LinkActive();
                }
            }
        }


        public Link(int cloudServer, int networkNodeServer)
        {
            socketBuilder = LocalSocektBuilder.Instance;
            this.Source = cloudServer;
            this.Destination = networkNodeServer;
            this.running = true;
        }

        public void StartListening()
        {
            try
            {
                listener = socketBuilder.getTcpSocket(Source);
                IsLinkActive = true;
                while (running)
                {
                    listener.Listen(1);
                    if (!IsLinkActive)
                    {
                        continue;
                    }
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptIncomingConnection), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                //TODO handling exception
            }
        }

        private void AcceptIncomingConnection(IAsyncResult ar)
        {   
            allDone.Set();
            try
            {
                Socket handler = ((Socket)ar.AsyncState).EndAccept(ar); // Tworzy Socket do obsługi przychodzącego
                StateObject state = new StateObject();
                state.workSocket = handler;
                // TODO sprawdzić czy nie będzie sytuacji w której spróbujemy stworzyć dwie wtyczki na tym samym porcie -> przy większej ilości kabli
                state.sender = socketBuilder.getTcpSocket();
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(StartReadingData), state);
            }
            catch (ObjectDisposedException connectionDisposed)
            {
                //TODO exception handling
            }
        }

        public void StartReadingData(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            Socket sender = state.sender;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                IPEndPoint endpoint = socketBuilder.getLocalEndpoint(Destination);
                try
                {
                    sender.BeginConnect(endpoint, new AsyncCallback(ConnectToNextNode), sender);
                    connectDone.WaitOne();

                    sendData(sender, state.buffer);
                    sendDone.WaitOne();
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (Exception ex)
                {

                }
            }
            //TODO wprowadzić jakąś sekwencje końcową albo ograniczoną liczbębajtówktórą będziemy przesyłać  
            //Tu może wystąpić błąd związany ze zbyt dużm blokeim danych dlatego przyda się else który wywoła jeszcze raz StartReadingData
        }

        private void ConnectToNextNode(IAsyncResult ar)
        {
            
            Socket sender = (Socket)ar.AsyncState;
            sender.EndConnect(ar);
            connectDone.Set();
            
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

        public void DestroyLink()
        {
            if (listener == null)
            {
                return; 
            } 

            //listener.Shutdown(SocketShutdown.Both);
            running = false;
            listener.Close();
        }
        

    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud.SocketUtils;

namespace WireCloud
{
    public delegate void LinkStateChangedHandler();
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
        public int Source { get; private set; }
        public int Destination { get; private set; }
        private bool running;
        private Socket listener;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        public event LinkStateChangedHandler LinkActive;

        private bool active;
        public bool IsLinkActive
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
                if (LinkActive != null)
                {
                    LinkActive();
                }
            }
        }


        public Link(int cloudServer, int networkNodeServer)
        {
            socketBuilder = LocalSocektBuilder.Instance;
            this.Source = cloudServer;
            this.Destination = networkNodeServer;
            this.running = true;
        }

        public void StartListening()
        {
            try
            {
                listener = socketBuilder.getTcpSocket(Source);
                IsLinkActive = true;
                while (running)
                {
                    listener.Listen(1);
                    if (!IsLinkActive)
                    {
                        continue;
                    }
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptIncomingConnection), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                //TODO handling exception
            }
        }

        private void AcceptIncomingConnection(IAsyncResult ar)
        {   
            allDone.Set();
            try
            {
                Socket handler = ((Socket)ar.AsyncState).EndAccept(ar); // Tworzy Socket do obsługi przychodzącego
                StateObject state = new StateObject();
                state.workSocket = handler;
                // TODO sprawdzić czy nie będzie sytuacji w której spróbujemy stworzyć dwie wtyczki na tym samym porcie -> przy większej ilości kabli
                state.sender = socketBuilder.getTcpSocket();
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(StartReadingData), state);
            }
            catch (ObjectDisposedException connectionDisposed)
            {
                //TODO exception handling
            }
        }

        public void StartReadingData(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            Socket sender = state.sender;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                IPEndPoint endpoint = socketBuilder.getLocalEndpoint(Destination);
                sender.BeginConnect(endpoint, new AsyncCallback(ConnectToNextNode), sender);
                connectDone.WaitOne();
                try
                {
                    sendData(sender, state.buffer);
                    sendDone.WaitOne();
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
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

        public void DestroyLink()
        {
            if (listener == null)
            {
                return; 
            } 

            //listener.Shutdown(SocketShutdown.Both);
            running = false;
            listener.Close();
        }
        

    }
}
>>>>>>> 92266fe8846c35516be6d7a5a1a402926158febf
