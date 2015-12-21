﻿
using NetworkNode.SDHFrame;
using NetworkNode.HPC;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDHClient
{
 
    public class ClientManagementCenter
    {
        ClientManagementPort managementPort;
        // NetworkNode.NetworkNode node;
        Adaptation adapt;

        public ClientManagementCenter(ClientManagementPort managementPort, Adaptation adapt)
        {
            this.adapt = adapt;
            this.managementPort = managementPort;
        }

        public string PerformManagementAction(string request)
        {
            string[] departedRequest = request.Split('|');
            int argLength = departedRequest.Length - 1;
            string requestType = departedRequest[0];
            List<List<string>> arguments = new List<List<string>>();

            for (int i = 1; i < departedRequest.Length; i++)
            {
                arguments.Add(new List<string>(departedRequest[i].Split('#')));
            }

            String response = "ERROR";
            switch (requestType)
            {
                case "get-ports":
                    {
                        response = getPortList();
                        break;
                    }
                case "identify":
                    {
                        response = identify();
                        break;
                    }
                case "resource-location":

                       response =  load(request);

                    break;
                case "resource - location":
                   response =  load(request);
                    break;

                case "resource- location":
                   response =  load(request);
                    break;
                case "resource -location":
                    response= load(request);
                    break;
                default:
                    Console.WriteLine("To nie powinno się zdarzyć: management prosi o: " + requestType);
                    break;
            }

            return response;
        }

        private string identify()
        {
            return "client|"+ adapt.client_name.ToString(); ;
        }

       
        private string getPortList()
        {

            Console.WriteLine("Klient: management żąda: get ports");
            string to_send = "";
            //reach (Input ii in adapt.links.input_ports) { to_send += ii.InputPort; to_send += "#"; }
            foreach (IOPort io in adapt.links.ports)
                to_send += io.listeningPort;
            to_send.Remove(to_send.Length - 1, 1);
            to_send += '|';
            foreach (IOPort io in adapt.links.ports) { to_send += io.PortTo; to_send += "#"; }
            List<byte> b = new List<byte>();
            return to_send;
        }
       
        private string load(string response) {
            string str = "";
            bool ok = true;
            try
            {
                str = response;
                int index1 = str.IndexOf('|');
                int index2 = str.IndexOf('#', index1);
                int index3 = str.IndexOf('#', index2 + 1);
                int index4 = str.IndexOf('#', index3 + 1);
                int index5 = str.IndexOf('#', index4 + 1);
                int index6 = str.IndexOf('#', index5 + 1);
                if (index1 != -1 && index2 != -1 && index3 != -1 && index4 != -1 && index5 != -1 && index6 != -1) ok = true;

                int port_in = Int32.Parse(str.Substring(index1 + 1, index2 - (index1 + 1)));
                int port_out = Int32.Parse(str.Substring(index2 + 1, index3 - (index2 + 1)));
                int from = Int32.Parse(str.Substring(index3 + 1, index4 - (index3 + 1)));
                int to = Int32.Parse(str.Substring(index4 + 1, index5 - (index4 + 1)));
                VirtualContainerLevel vc = VirtualContainerLevel.UNDEF ;
                StmLevel stm = StmLevel.UNDEF;
               
                string level = str.Substring(index5 + 1, index6 - (index5 + 1));
                switch (level)
                {
                    case "VC12": vc = VirtualContainerLevel.VC12; break;
                    case "VC32": vc = VirtualContainerLevel.VC32; break;
                    case "VC21": vc = VirtualContainerLevel.VC21; break;
                    case "VC4": vc = VirtualContainerLevel.VC4; break;

                }
                string level2 = str.Substring(index6 + 1, str.Length - (index6 + 1));
                switch (level2)
                {
                    case "STM16": stm = StmLevel.STM16; break;
                    case "STM1": stm = StmLevel.STM1; break;
                    case "STM256": stm = StmLevel.STM256; break;
                    case "STM4": stm = StmLevel.STM4; break;
                    case "STM64": stm = StmLevel.STM64; break;


                }
                adapt.connections.Clear();
                adapt.connections.Add(new ConnInfo(port_in, port_out, from, to, vc,stm));
                 Console.WriteLine("Klient: przyjęto port_in{0}, port_out{1}, poziom_z{2},poziom_do{3},kontener{4},trakt{5} ", port_in, port_out, from, to, level,level2);
                return "OK";
            }
            catch (Exception) { return "ERROR"; }
        }
        

    }
}
