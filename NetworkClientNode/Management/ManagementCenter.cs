
using NetworkNode.SDHFrame;
using NetworkNode.HPC;
using NetworkNode.Ports;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkClientNode.Adaptation;

namespace NetworkClientNode.Menagment
{

    public class ManagementCenter
    {
        ManagementClientPort managementPort;
        NetworkClNode node;

        public ManagementCenter(ManagementClientPort managementPort, NetworkClNode node)
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
                case "resource-relocation":
                    {
                        response = addStreamData(arguments);
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
            return "client|" + node.Id.ToString(); ;
        }

        private string closeConnection(List<List<string>> connections)
        {
            if (connections.Count != 1)
            {
                return "ERROR: choose one connection";
            }

            List<string> literalRecord = connections[0];

            StreamData record = createRecord(literalRecord);

            return node.RemoveStreamData(record) ? "OK" : "ERROR";
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
            List<StreamData> records = node.GetStreamData();
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach (StreamData record in records)
            {
                builder.Append(record.Port);
                builder.Append("#");
                builder.Append(record.Stm.ToString());
                builder.Append("#");
                builder.Append(record.VcLevel.ToString());
                builder.Append("#");
                builder.Append(record.HigherPath);
                builder.Append("#");
                builder.Append(record.LowerPath == null ? "" : "" + record.LowerPath);
                if (index < records.Count - 1)
                {
                    builder.Append("|");
                }

                index++;
            }

            return builder.ToString();
        }

        private string addStreamData(List<List<string>> literalRecords)
        {
            List<StreamData> records = new List<StreamData>();
            foreach (List<string> literalRecord in literalRecords)
            {

                records.Add(createRecord(literalRecord));
            }
            ExecutionResult result = node.AddStreamData(records);

            return result.Result ? "OK" : "ERROR " + result.Msg;
        }

        private StreamData createRecord(List<string> literalRecord)
        {
            int outPort = int.Parse(literalRecord[0]);
            int? lowerPathOut = literalRecord[5].Equals("") ? null : (int?)int.Parse(literalRecord[5]);
            int higherPathOut = int.Parse(literalRecord[6]);

            VirtualContainerLevel level = VirtualContainerLevelExt.GetContainer(literalRecord[2]);
            StmLevel stm = StmLevelExt.GetContainer(literalRecord[5]);

            StreamData record = new StreamData(outPort, stm, level, higherPathOut, lowerPathOut);

            return record;
        }

    }
}
