using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using LRM;


/*public class StateObject
{
    public Socket WorkSocket { get; set; }
    public Action<string> Callback { get; set; }
    public byte[] Buffer { get; set; }
    public StringBuilder Sb { get; set; }
    
    public StateObject()
    {
        Sb = new StringBuilder();
    }
}*/

public class LrmClientServer : LocalPort
{
    private int Port;
    private static ManualResetEvent ConnectDone = new ManualResetEvent(false);
    private AsyncCommunication Async;
    private Action<string> DataRedCallback;
    private Thread ReciverThread;

    private const int BUFFER_SIZE = 500;

    public LrmClientServer(int port, Action<string> callback)
        : base(port)
    {
        DataRedCallback = callback;
    }

    public void ConnectToLrm()
    {
        lock (Async)
        {
            if (Async != null)
            {
                return;
            }
            StateObject state = new StateObject();
            ActionSocket.BeginConnect(Endpoint, new AsyncCallback(ConnectCallback), null);
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

    public void SendToLrm(string msg)
    {
        Async.Send(msg);
    }

}