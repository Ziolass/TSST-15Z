
using NetworkNode.SDHFrame;
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
                        response = getPortList();
                        break;
                case "identify":
                        response = identify();
                        break;
                case "delete-connection":
                    try { string s = arguments[0][0]; response = delete(s); } catch (Exception) { }
                    
                    break;
                case "get-connections":
                    response = getconn();
                break;

                case "resource-location":
                    response = relocate(arguments);
                break;
                case "resource-relocation":
                    response = relocate(arguments);
                    break;
                default:
                    Console.WriteLine("To nie powinno się zdarzyć: management prosi o: " + requestType);
                    break;
            }
           Console.WriteLine("CLIENT->MANAGEMENT: " + response);
            return response;
        }

        private string relocate(List<List<string>> arguments)
        {
            string msg = "", msg_total = "";
            foreach (List<string> list in arguments)
            {
                msg = "";
                try
                {

                    int port_in = Int32.Parse(list[0]);
                    int port_out = Int32.Parse(list[1]);
                    bool exists1 = false, exists2 = false;

                    
                    int from = Int32.Parse(list[2]);
                    int to = Int32.Parse(list[3]);
                    if (true)
                    {
                        //if(ConnInfo.levels_out.Contains(to)) { return "ERROR: OUT LEVEL " + to + "ALREADY IN USE"; }
                        VirtualContainerLevel vc = VirtualContainerLevel.UNDEF;
                        StmLevel stm = StmLevel.UNDEF;


                        switch (list[4])
                        {
                            case "VC12": vc = VirtualContainerLevel.VC12; break;
                            case "VC32": vc = VirtualContainerLevel.VC32; break;
                            case "VC21": vc = VirtualContainerLevel.VC21; break;
                            case "VC4": vc = VirtualContainerLevel.VC4; break;

                        }

                        switch (list[5])
                        {
                            case "STM16": stm = StmLevel.STM16; break;
                            case "STM1": stm = StmLevel.STM1; break;
                            case "STM256": stm = StmLevel.STM256; break;
                            case "STM4": stm = StmLevel.STM4; break;
                            case "STM64": stm = StmLevel.STM64; break;


                        }
                        adapt.stmlvl = stm;
                        int counter = -1, counter_out = -1, counter_in = -1;
                        foreach (IOPort io in adapt.links.ports)
                        {
                            counter++;

                            if (io.PortTo == port_out) counter_out = counter;
                            if (io.listeningPort == port_in) counter_in = counter;
                        }
                        //FrameBusiness in_frame=new FrameBusiness(), out_frame=new FrameBusiness();

                        /*  if (counter_in != -1 && ConnInfo.business.TryGetValue(port_in, out  in_frame)&&in_frame.is_port_in==true)
                          {
                              bool canI = in_frame.frame.SetVirtualContainer(vc,from,packUp(vc));
                              if (canI == false) msg = "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + from + " ,PORT:" + port_in;

                          }*/
                        Frame f;
                        Frame f2;
                       // Dictionary<int, FrameBusiness> temp = DictCopy.DeepCopy<int, FrameBusiness>(ConnInfo.business);
                        if (ConnInfo.business.ContainsKey(port_in) == false)
                        {
                             f = new Frame();
                            bool canI = f.SetVirtualContainer(vc, from, null, packUp(vc));
                            if (!canI)
                                msg = "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + from + " ,PORT:" + port_in;

                        }
                        else
                        {
                            f = new Frame(ConnInfo.business[port_in].frame);
                            bool canI = f.SetVirtualContainer(vc, from, null, packUp(vc)); 
                            if (!canI) 
                            msg = "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + from + " ,PORT:" + port_in;
                        }


                        if (ConnInfo.business.ContainsKey(port_out) == false)
                        {
                            f2 = new Frame();
                            bool canI = f.SetVirtualContainer(vc, from, null, packUp(vc)); 
                            if (!canI) if (!canI) msg = "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + to + " ,PORT:" + port_out;

                        }
                        else
                        {
                            f2 = new Frame(ConnInfo.business[port_out].frame);
                            bool canI = f.SetVirtualContainer(vc, from, null, packUp(vc)); 
                            if (!canI) msg = "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + to + " ,PORT:" + port_out;

                        }

                        if(msg == "")
                        {
                            if (ConnInfo.business.ContainsKey(port_out) == false)  ConnInfo.business.Add(port_out, new FrameBusiness(false, f2));
                            else ConnInfo.business[port_out].frame = f2;
                            if(ConnInfo.business.ContainsKey(port_in) == false) ConnInfo.business.Add(port_in, new FrameBusiness(true, f));
                            else ConnInfo.business[port_in].frame = f;
                            adapt.connections.Add(new ConnInfo(port_in, port_out, from, to, vc, stm));
                            ConnInfo.levels_in.Add(from);
                            ConnInfo.levels_out.Add(to);
                            refresh();
                        }
                        else
                        {
                            msg_total += msg+"\n";
                        }
                        /* if (counter_in != -1 && ConnInfo.business.TryGetValue(port_out, out out_frame)&&out_frame.is_port_in == false)
                         {
                             bool canI = out_frame.frame.SetVirtualContainer(vc, to, packUp(vc));
                             if (canI == false) msg= "ERROR: THIS LEVEL IN THIS CONTAINER IS INACCESSIBLE OR CANT CREATE CONTAINER:" + vc.ToString() + ", LEVEL:" + from + " ,PORT:" + port_in;

                         }*/

                        /*if (ConnInfo.business.ContainsKey(port_out) == false) {
                            Frame f = new Frame();
                           bool canI = f.SetVirtualContainer(vc, to, packUp(vc));
                            if (!canI) msg = "ERROR: CANT CREATE CONTAINER";
                            else ConnInfo.business.Add(port_out, new FrameBusiness(false,f));
                        }*/

                        
                        
                        //ConnInfo.business = DictCopy.DeepCopy<int, FrameBusiness>(temp);
                       
                    }
                   // else return "ERROR:PORT"+port_in+" LUB "+port_out+" NIEDOSTEPNY "; 

                }
                catch(DivideByZeroException e)
                {
                    return "ERROR:PARSING";
                }



            }

            if (msg_total == "") msg_total = "OK";
            adapt.refreshConnections();
            refresh();
            return msg_total;
        }
        public void refresh()
        {
            //najpierw usuwamy dany elelemt!!!


            ConnInfo.business.Clear();
            for (int a = 0; a < adapt.connections.Count; a++)
            {
                int port_out = adapt.connections[a].port_out;
                int port_in = adapt.connections[a].port_in;
                if (!ConnInfo.business.ContainsKey(port_out))
                    ConnInfo.business.Add(adapt.connections[a].port_out, new FrameBusiness(false, new Frame()));
                if (!ConnInfo.business.ContainsKey(port_in))
                    ConnInfo.business.Add(adapt.connections[a].port_in, new FrameBusiness(true, new Frame()));
                Frame f1 = ConnInfo.business[port_in].frame;
                Frame f2 = ConnInfo.business[port_out].frame;
                f1.SetVirtualContainer(adapt.connections[a].level, adapt.connections[a].level_from, null, packUp(adapt.connections[a].level));
                f2.SetVirtualContainer(adapt.connections[a].level, adapt.connections[a].level_to, null,  packUp(adapt.connections[a].level));
                ConnInfo.business[port_in].frame = f1;
                ConnInfo.business[port_out].frame = f2;

            }

        }
        
        private VirtualContainer packUp(VirtualContainerLevel level)
        {
            int size = 0;
            if (level == VirtualContainerLevel.VC12)
            {
                size = 28;
            }
            if (level == VirtualContainerLevel.VC21)
            {
                size = 11 * 9;
            }
            if (level == VirtualContainerLevel.VC32)
            {
                size = 83 * 9;
            }
            if (level == VirtualContainerLevel.VC4)
            {
                size = 261;
            }
            else
            {
                size = 28;
            }
            VirtualContainer newVC = new VirtualContainer(level);
            newVC.SetContent(new NetworkNode.SDHFrame.Container(generateString(size))); //sobczakj
            return newVC;



        }
        private string generateString(int size)
        {
            string ret = "";
            for (int a = 0; a < size; a++) ret += 'a';
            return ret;
        }
        private string delete(string who)
        {
            for(int a=0;a<adapt.connections.Count;a++)
            {
                if(adapt.connections[a].identifier == who)
                {
                    adapt.connections.RemoveAt(a); refresh(); adapt.refreshConnections();  return "OK";
                }
            }

                return "ERROR: IDENTIFIER NOT FOUND";
        }

        private string getconn()
        {
            string t = "";
            foreach(ConnInfo ci in adapt.connections)
            {
                t += ci.identifier + "#" + ci.port_in + "#" + ci.port_out + "#" + ci.level_from + "#" + ci.level_to + "#" + ci.level.ToString() + "#" + ci.stmlevel.ToString() + "|";
            }
            if (adapt.connections.Count > 0) return t.Remove(t.Length - 1, 1);
            else return "";
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
            { to_send += io.listeningPort; to_send += "#"; }
              to_send =   to_send.Remove(to_send.Length - 1, 1);
            to_send += '|';
            foreach (IOPort io in adapt.links.ports) { to_send += io.PortTo; to_send += "#"; }
           to_send = to_send.Remove(to_send.Length - 1, 1);
            //List<byte> b = new List<byte>();
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
                //adapt.connections.Clear();
                adapt.connections.Add(new ConnInfo(port_in, port_out, from, to, vc,stm));
                 Console.WriteLine("Klient: przyjęto port_in{0}, port_out{1}, poziom_z{2},poziom_do{3},kontener{4},trakt{5} ", port_in, port_out, from, to, level,level2);
                return "OK";
            }
            catch (Exception) { return "ERROR"; }
        }
        

    }
    public static class DictCopy
    {
        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            Dictionary<TKey, TValue> d2 = new Dictionary<TKey, TValue>();

            bool keyIsCloneable = default(TKey) is ICloneable;
            bool valueIsCloneable = default(TValue) is ICloneable;

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                TKey key = default(TKey);
                TValue value = default(TValue);
                if (keyIsCloneable)
                {
                    key = (TKey)((ICloneable)(kvp.Key)).Clone();
                }

                else
                {
                    key = kvp.Key;
                }

                if (valueIsCloneable)
                {
                    value = (TValue)((ICloneable)(kvp.Value)).Clone();
                }

                else
                {
                    value = kvp.Value;
                }

                d2.Add(key, value);
            }

            return d2;
        }
    }
}
