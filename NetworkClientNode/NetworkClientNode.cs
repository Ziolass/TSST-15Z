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
        public int SelectedStream { get; private set; }

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

        public void RegisterDataListener(HandleClientData clientDataDelegate)
        {
            Adaptation.HandleClientData += clientDataDelegate;
        }
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendData(string message)
        {
            Dictionary<StreamData, string> tempDictionary = new Dictionary<StreamData, string>();
            tempDictionary.Add(this.Adaptation.GetStreamData().ElementAt<StreamData>(this.SelectedStream), message);
            this.Adaptation.SentData(tempDictionary);
        }
        /// <summary>
        /// Selects the stream for sending messages.
        /// </summary>
        /// <param name="streamData">The stream data.</param>
        public void SelectStream(StreamData streamData)
        {
            if (GetStreamData() != null && GetStreamData().Contains(streamData))
            {
                this.SelectedStream = GetStreamData().IndexOf(streamData);
            }
        }
    }
}
