using NetworkClientNode.Adaptation;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode
{
    public class NetworkClNode
    {
        private AdaptationFunction Adaptation;

        public string Id { get; set; }

        public NetworkClNode(AdaptationFunction adaptation)
        {
            Adaptation = adaptation;
        }

        public Dictionary<int, StmLevel> GetPorts()
        {
            return Adaptation.GetPorts();
        }

        public List<StreamData> GetStreamData()
        {
            return Adaptation.GetStreamData();
        }

        public ExecutionResult AddStreamData(List<StreamData> records)
        {
            return Adaptation.AddStreamData(records);
        }

        public  bool RemoveStreamData(StreamData record)
        {
            return Adaptation.RemoveStreamData(record);
        }
    }
}
