using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SDHManagement2.Models;
using SDHManagement2.AdditionalWindows;
using SDHManagement2.FileUtils;

namespace SDHManagement2.SocketUtils
{
    public class SocketHandler
    {
        private MainWindow mainWindow;
        private List<Router> rList;
        private List<Router> cList;
        public List<string> nodelist { get; set; }
        public List<string> clientNameList { get; set; }

        public SocketHandler(List<Router> routerList, List<Router> clientList, MainWindow main)
        {
            rList = routerList;
            cList = clientList;
            mainWindow = main;
            nodelist = new List<string>();
            clientNameList = new List<string>();
            refresh();


        }

        public RouterSocket GetRouterSocket(String name)
        {
            if (rList == null)
            {
                return null;
            }

            foreach (var router in rList)
            {
                if (router.identifier.Equals(name))
                {
                    return router.socket;
                }

            }
            foreach (var client in cList)
            {
                if (client.identifier.Equals(name))
                {
                    return client.socket;
                }
            }
            return null;
        }
        public RouterSocket getClientSocket(String name)
        {
            if (cList == null)
            {
                return null;
            }

            foreach (var client in cList)
            {
                if (client.identifier.Equals(name))
                {
                    return client.socket;
                }

            }
            return null;
        }

        public String sendCommand(String name, String command,bool append)
        {

            

            RouterSocket targetSocket = GetRouterSocket(name);
            byte [] bytes = new byte[100000];
            string response=null;
            targetSocket.InitConversation();
            Socket conversationSocket = targetSocket.GetSocket();
            
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(command);

                conversationSocket.Send(msg);
                
                //while (true)
                {

                    bytes=new byte[1024];
                    int bytesRec = conversationSocket.Receive(bytes);
                    response += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (append)
                    {
                        mainWindow.appendConsole(response, name, command);
                    }
                   // if (response.IndexOf("<EOF>") > -1)
                    //{
                    //    break;
                    //}
                }

                

            }
            catch (Exception e)
            {
                mainWindow.appendConsole("Command could not be send. Try again",null,null);
            }
            // TODO zmienić to
            return response;
        }

        public string commandHandle(string command, string node)
        {
            if (node.Length < 1)
            {
                mainWindow.appendConsole("None node specified, try again", null, null);
                return null;
            }
            switch (command)
            {
                case "resource-location":
                    string portresponse = sendCommand(node, "get-ports|", false);
                    ResourceRelocationWindow relocationwindow = new ResourceRelocationWindow(this, node,portresponse);
                    relocationwindow.ShowDialog();
                    return "DONE";
                case "disable-node":
                    return sendCommand(node, command+"|",true);

                case "shutdown-interface":
                    string connresponse = sendCommand(node, "get-connection-list|",false);

                    DropConnectionWindow dropWindow = new DropConnectionWindow(this,connresponse,node,mainWindow);
                    dropWindow.ShowDialog();
                    return "DONE";

                case "sub-connection-HPC":
                    string port_response = sendCommand(node, "get-ports|",false);
                    string connectionresponse = sendCommand(node, "get-connection-list|",false);
                    AddConnectionWindow window = new AddConnectionWindow(this,port_response,connectionresponse,node);
                    window.ShowDialog();
                    return "DONE";

                case "get-connection-list":
                    return sendCommand(node, command + "|",true);

                case "get-ports":
                    return sendCommand(node, command + "|",true);

                default:
                    mainWindow.appendConsole("Unimaginable happened",null,null);
                    return "ERROR";

            }
        }
        public void responseHandle(string response)
        {

        }
        public void refresh()
        {
            //List<String> nodeList = new List<string>();

            foreach (var router in rList)
            {
                if (!router.connected)
                { 
                    mainWindow.appendConsole(string.Format("Trying to reach {0} on port {1}", router.identifier,
                        router.port),null,null);

                try
                {
                    router.socket = new RouterSocket(router.port, router.identifier);
                    router.socket.TurnOn();
                    router.connected = true;
                    nodelist.Add(router.identifier);
                    mainWindow.appendConsole(string.Format("Connection to {0} succeeded", router.identifier),null,null);

                }
                catch (Exception e)
                {
                    mainWindow.appendConsole("Could not connect to: " + router.identifier,null,null);
                }
            }
        }
            foreach (var client in cList)
            {
                if (!client.connected)
                {
                    mainWindow.appendConsole(string.Format("Trying to reach {0} on port {1}", client.identifier,
                        client.port), null, null);

                    try
                    {
                        client.socket = new RouterSocket(client.port, client.identifier);
                        client.socket.TurnOn();
                        client.connected = true;
                        clientNameList.Add(client.identifier);
                        mainWindow.appendConsole(string.Format("Connection to {0} succeeded", client.identifier), null, null);

                    }
                    catch (Exception e)
                    {
                        mainWindow.appendConsole("Could not connect to: " + client.identifier, null, null);
                    }
                }
            }

            //mainWindow.nodeBox.ItemsSource = nodelist;
            
        }
        public string addSingleNode(string name, int port_)
        {
            try {

                Router r = new Router();
                r.identifier = name;
                r.port = port_;
                r.socket = new RouterSocket(r.port, r.identifier);
                r.socket.TurnOn();
                r.connected = true;
                nodelist.Add(r.identifier);
                
                rList.Add(r);
                mainWindow.nodeBox.ItemsSource = nodelist;

                mainWindow.appendConsole(string.Format("Connection to {0} succeeded", r.identifier), null, null);

            }
            catch (Exception e)
            {
                return "Error while trying to connect to "+name;
            }
            try
            {
                // TODO
               // ConfigReader.addNewElement(name, port_);
            }
            catch(Exception e)
            {
                return "Error while trying to add to config file";
            }
            return name + " added";
        }
    }
}
