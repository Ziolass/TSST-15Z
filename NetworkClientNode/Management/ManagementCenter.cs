
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
                        response = AddStreamData(arguments);
                        break;
                    }
                case "get-resource-list":
                    {
                        response = GetResourceList();
                        break;
                    }
                case "get-ports":
                    {
                        response = GetPortList();
                        break;
                    }
                case "identify":
                    {
                        response = Identify();
                        break;
                    }
                case "delete-resource":
                    {
                        response = CloseConnection(arguments);
                        break;
                    }
            }

            return response;
        }

        private string Identify()
        {
            return "client|" + node.Id.ToString(); ;
        }

        private string CloseConnection(List<List<string>> connections)
        {
            if (connections.Count != 1)
            {
                return "ERROR: choose one connection";
            }

            List<string> literalRecord = connections[0];

            StreamData record = CreateRecord(literalRecord);

            return node.RemoveStreamData(record) ? "OK" : "ERROR";
        }

        private string GetPortList()
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
                index++;
            }

            return builder.ToString(); ;
        }
        private string GetResourceList()
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

        private string AddStreamData(List<List<string>> literalRecords)
        {
            List<StreamData> records = new List<StreamData>();
            foreach (List<string> literalRecord in literalRecords)
            {
                records.Add(CreateRecord(literalRecord));
            }
            ExecutionResult result = node.AddStreamData(records);

            return result.Result ? "OK" : "ERROR " + result.Msg;
        }

        private StreamData CreateRecord(List<string> literalRecord)
        {
            int outPort = int.Parse(literalRecord[0]);
            int? lowerPath = literalRecord[4].Equals("") ? null : (int?)int.Parse(literalRecord[4]);
            int higherPath = int.Parse(literalRecord[3]);

            VirtualContainerLevel level = VirtualContainerLevelExt.GetContainer(literalRecord[2]);
            StmLevel stm = StmLevelExt.GetContainer(literalRecord[1]);

            StreamData record = new StreamData(outPort, stm, level, higherPath, lowerPath);

            return record;
        }

    }
}
