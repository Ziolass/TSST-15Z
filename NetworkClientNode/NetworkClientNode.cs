using NetworkClientNode.Adaptation;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode
{
    public delegate void StreamChangedHandler(StreamChangedArgs args);

    public class StreamChangedArgs : EventArgs
    {
        public List<StreamData> Streams { get; private set; }
        public StreamChangedArgs(List<StreamData> stream)
        {
            Streams = stream;
        }
    }
    public class NetworkClNode
    {
        private AdaptationFunction Adaptation;
        public int SelectedStream { get; private set; }
        public string Id { get; set; }
        public event StreamChangedHandler StreamAdded;
        public event StreamChangedHandler StreamRemoved;

        public NetworkClNode(AdaptationFunction adaptation, string id)
        {
            Adaptation = adaptation;
            Id = id;
        }

        public Dictionary<int, StmLevel> GetPorts()
        {
            return Adaptation.GetPorts();
        }

        public List<StreamData> GetStreamData()
        {
            return Adaptation.GetStreamData();
        }

        public ExecutionResult AddStreamData(List<StreamData> streams)
        {
            ExecutionResult executionResult = Adaptation.AddStreamData(streams);
            if (executionResult.Result)
                if (StreamAdded != null)
                {
                    StreamAdded(new StreamChangedArgs(streams));
                }
            return executionResult;
        }

        public  bool RemoveStreamData(StreamData stream)
        {
            if (Adaptation.RemoveStreamData(stream))
            {
                if (StreamRemoved != null)
                {
                    List<StreamData> streams = new List<StreamData>();
                    streams.Add(stream);
                    StreamRemoved(new StreamChangedArgs(streams));
                }
                return true;
            }
            else return false;
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
