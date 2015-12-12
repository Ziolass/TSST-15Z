
using NetworkNode.HPC;
using NetworkNode.Ports;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.MenagmentModule
{
    enum menagementProtocol{

    }

    public class ManagementCenter
    {
        ManagementPort managementPort;
        NetworkNode node;

        public ManagementCenter(ManagementPort managementPort, NetworkNode node)
        {
            this.node = node;
            this.managementPort = managementPort;
        }

        public string PerformManagementAction(string request)
        {
            string[] departedRequest = request.Split('|');
            string[] requestArguments = {};
            int argLength = departedRequest.Length - 1;
            string requestType = departedRequest[0];
            
            if (argLength > 0)
            {
                requestArguments = new string[argLength];
                Array.Copy(departedRequest, 1, requestArguments, 0, argLength);
            }
            String response = "ERROR";
            switch (requestType)
            {
                case "disable-node":
                    {
                        response = disableNode();
                        break;
                    }
                case "shutdown-interface":
                    {
                        response = shutdownInterface(requestArguments);
                        break;
                    }
                case "sub-conection-HPC":
                    {
                        response = addForwardingRecord(requestArguments);
                        break;
                    }
                case "get-connection-list":
                    {
                        response = getConnectionList();
                        break;
                    }
                case "get-ports":
                    {
                        response = getPortList();
                        break;
                    }       

            }

            return response;
        }

        private string disableNode()
        {
            return node.DisableNode() ? "OK" : "ERROR";
        }
        private string shutdownInterface(string[] testPort)
        {
            int port = int.Parse(testPort[0]);
            return node.ShudownInterface(port) ? "OK" : "ERROR";
        }
        private string getPortList()
        {
            List<List<int>> inOutPorts = node.GetPorts();
            StringBuilder builder = new StringBuilder();
            int mainIndex = 0;
            foreach (List<int> ports in inOutPorts)
            {
                int index = 0;
                foreach (int port in ports)
                {

                    builder.Append(port);
                    if (index < ports.Count - 1)
                    {
                        builder.Append("#");
                    } 
                }

                if (mainIndex < inOutPorts.Count - 1)
                {
                    builder.Append("|");
                }
            }
            return builder.ToString(); ;
        }
        private string getConnectionList()
        {
            List<ForwardingRecord> records = node.GetForwardingRecords();
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (ForwardingRecord record in records)
            {
                builder.Append(record.InputPort);
                builder.Append("#");
                builder.Append(record.OutputPort);
                builder.Append("#");
                builder.Append(record.VcNumberIn);
                builder.Append("#");
                builder.Append(record.VcNumberOut);
                builder.Append("#");
                builder.Append(record.ContainerLevel);
                
                if (index < records.Count-1)
                {
                    builder.Append("|");
                }
                
                index++;
            }

            return builder.ToString();
        }

        private string addForwardingRecord(StringBuilder sb)
        {
            /*if (pair == null)
            {
                sb.Append("?");
                return;
            }

            sb.Append(pair.inputIdentifier);
            sb.Append("-");
            sb.Append(pair.outputIdentifier);*/
            return null;
        }

        private string establishLink(String[] requestArguments)
        {
            /*foreach(String connection in requestArguments){
                if (connection.Equals("")) { continue; }
                String[] multiLevelSwitching = connection.Split('#');
                

                if (multiLevelSwitching.Length > 2 || multiLevelSwitching.Length <1)
                {
                   // throw Exception();
                }
                String interfaceLevelConnection = multiLevelSwitching[0];
                String containerLevelConnection = multiLevelSwitching.Length == 2 && multiLevelSwitching[1] != ""? multiLevelSwitching[1] : null;
                
                InOutPair interfacePair = transformToPair(interfaceLevelConnection,MultiplexationLevel.SLOTS);
                //TODO to trzeba zminić multiplexation level musi być generyczny
                //InOutPair containerPair = transformToPair(containerLevelConnection, MultiplexationLevel.VC4);

                ForwardingRecord muxConnection = new ForwardingRecord(interfacePair, null);
                hpc.addConnection(muxConnection);
                
            }*/
            return null;
        }

        /*private InOutPair transformToPair(String pair, MultiplexationLevel muxLevel)
        {
            if (pair == null || pair.Equals(""))
            {
                return null;
            }
            InOutPair result = new InOutPair();
            String[] partedPair = pair.Split('-');
            result.inputIdentifier = int.Parse(partedPair[0]);
            result.outputIdentifier = int.Parse(partedPair[1]);
            result.level = muxLevel;
            return result;
        }

        private void sendPortList()
        {
            Dictionary<int, IoSlot> interfaces = ttf.getInterfaces();
            StringBuilder sb = new StringBuilder();
            if(interfaces.Count > 0) {
                sb.Append("OK|");
            } else {
                sb.Append("ERROR|Any of ports doesn't exist");
            }

            foreach (KeyValuePair<int, IoSlot> networkInterface in interfaces)
            {
                sb.Append("interface:[");
                sb.Append(networkInterface.Key);
                sb.Append("],in:[");
                sb.Append(networkInterface.Value.getIputPort());
                sb.Append("].out:[");
                sb.Append(networkInterface.Value.getOutputPort());
                sb.Append("]|");
            }
            managmentInterface.sendFrameBytes(sb.ToString());
        } */
        private List<int> prapreInterfaceList(string[] departedRequest)
        {
            List<int> interfacesToShut = new List<int>();
            string[] ports= new string[departedRequest.Length-1];
            Array.Copy(departedRequest, 1, ports, 0, departedRequest.Length - 1);

            foreach(String stringNumber in ports){
                try
                {
                    interfacesToShut.Add(Int32.Parse(stringNumber));
                }
                catch (Exception ex)
                {

                }
            }
            return interfacesToShut;
        }
    }
}
