using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


public class StateObject
{
    public Socket WorkSocket { get; set; }
    public Action<string> Callback { get; set; }
    public byte[] Buffer { get; set; }
    public StringBuilder Sb { get; set; }
    
    public StateObject()
    {
        Sb = new StringBuilder();
    }
}

public class LrmClientServer
{
    private int Port;
    private static ManualResetEvent ConnectDone = new ManualResetEvent(false);

    private const int BUFFER_SIZE = 500;

    public LrmClientServer(int port)
    {
        Port = port;
    }

    public void SendMessage(string LrmMessage, Action<string> callback)
    {
        try
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint remoteEP = InitEndpoint();
            client.BeginConnect(remoteEP,new AsyncCallback(ConnectCallback), client);
            
            ConnectDone.WaitOne();
            Send(client, LrmMessage, callback);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            ConnectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(StateObject state)
    {
        try
        {
            state.Buffer = new byte[BUFFER_SIZE];
            state.WorkSocket.BeginReceive(state.Buffer, 0, BUFFER_SIZE, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.WorkSocket;
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                client.BeginReceive(state.Buffer, 0, BUFFER_SIZE, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {

                if (state.Sb.Length > 1)
                {
                    state.Callback(state.Sb.ToString());
                }

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data, Action<string> callback)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        
        StateObject state = new StateObject
        {
            Callback = callback,
            WorkSocket = client
        };

        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), state);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            int bytesSent = state.WorkSocket.EndSend(ar);
            Receive(state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private IPEndPoint InitEndpoint()
    {
        IPHostEntry ipHostInfo = Dns.Resolve("127.0.0.1");
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

        return remoteEP;
    }
}