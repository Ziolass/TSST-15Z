
using NetworkNode.SDHFrame;
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
                case "disable-node":
                    {
                        response = disableNode();
                        break;
                    }
                case "shutdown-interface":
                    {
                        response = shutdownInterface(arguments);
                        break;
                    }
                case "sub-connection-HPC":
                    {
                        response = addForwardingRecord(arguments);
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
                case "identify":
                    {
                        response = identify();
                        break;
                    }
                case "close-connection":
                    {
                        response = closeConnection(arguments);
                        break;
                    }

            }

            return response;
        }

        private string identify()
        {
            return "router|" + node.Id.ToString(); ;
        }

        private string closeConnection(List<List<string>> connections)
        {
            if (connections.Count != 1)
            {
                return "ERROR: choose one connection";
            }

            List<string> literalRecord = connections[0];

            List<ForwardingRecord> records = createRecord(literalRecord);

            return node.RemoveTwWayRecord(records)  ? "OK" : "ERROR";
        }

        private string disableNode()
        {
            return node.DisableNode() ? "OK" : "ERROR";
        }

        private string shutdownInterface(List<List<string>> testPort)
        {

            int port = int.Parse(testPort[0][0]);
            return node.ShudownInterface(port) ? "OK" : "ERROR";
        }

        private string getPortList()
        {
            Dictionary<int, StmLevel> ports = node.GetPorts();
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (int port in ports.Keys)
            {

                builder.Append(port);
                builder.Append("#");
                builder.Append(ports[port].ToString());
                if (index < ports.Count - 1)
                {
                    builder.Append("|");
                }
            }

            return builder.ToString(); ;
        }
        private string getConnectionList()
        {
            List<ForwardingRecord> records = node.GetConnections();
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (ForwardingRecord record in records)
            {
                builder.Append(record.InputPort);
                builder.Append("#");
                builder.Append(record.OutputPort);
                builder.Append("#");
                builder.Append(record.ContainerLevel.ToString());
                builder.Append("#");
                builder.Append(record.VcNumberIn == -1 ? "" : "" + record.VcNumberIn);
                builder.Append("#");
                builder.Append(record.HigherPathIn);
                builder.Append("#");
                builder.Append(record.VcNumberOut == -1 ? "" : "" + record.VcNumberOut);
                builder.Append("#");
                builder.Append(record.HigherPathOut);
                if (index < records.Count - 1)
                {
                    builder.Append("|");
                }

                index++;
            }

            return builder.ToString();
        }

        private string addForwardingRecord(List<List<string>> literalRecords)
        {
            List<List<ForwardingRecord>> records = new List<List<ForwardingRecord>>();
            foreach (List<string> literalRecord in literalRecords)
            {
                records.Add(createRecord(literalRecord));
            }
            ExecutionResult result = node.AddForwardingRecords(records);

            return result.Result ? "OK" : "ERROR " + result.Msg;
        }

        private List<ForwardingRecord> createRecord(List<string> literalRecord)
        {

            int port1 = int.Parse(literalRecord[0]);
            int port2 = int.Parse(literalRecord[1]);
            int lPath1 = int.Parse(literalRecord[3].Equals("") ? "-1" : literalRecord[3]);
            int lPath2 = int.Parse(literalRecord[5].Equals("") ? "-1" : literalRecord[5]);
            int hPath1 = int.Parse(literalRecord[4]);
            int hPath2 = int.Parse(literalRecord[6]);
            VirtualContainerLevel level = VirtualContainerLevelExt.GetContainer(literalRecord[2]);
            //StmLevel stm = StmLevelExt.GetContainer(literalRecord[5]);
            List<ForwardingRecord> forwardingRecords = new List<ForwardingRecord>();
            forwardingRecords.Add(new ForwardingRecord(port1, port2, level, lPath1, lPath2, hPath1, hPath2));
            forwardingRecords.Add(new ForwardingRecord(port2, port1, level, lPath2, lPath1, hPath2, hPath1));

            return forwardingRecords;
        }

    }
}
