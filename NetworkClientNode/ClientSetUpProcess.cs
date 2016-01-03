using NetworkClientNode.Adaptation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode
{
    public delegate void StreamsCreatedHandler();
    public delegate void StreamCreatedHandler(StreamCreatedArgs arg);

    public class StreamCreatedArgs : EventArgs
    {
        public StreamCreatedArgs(StreamData streamData)
        {
            StreamData = streamData;
        }
        public StreamData StreamData { get; private set; }
    }
    public class ClientSetUpProcess
    {
        private string ConfigurationFilePath;
        public ElementConfigurator ElementConfigurator { get; private set; }
        public NetworkClNode ClientNode { get; private set; }
        public event StreamsCreatedHandler StreamsCreated;
        public event StreamCreatedHandler StreamCreated;

        public ClientSetUpProcess(string configurationFilePath)
        {
            this.ConfigurationFilePath = configurationFilePath;
        }
        /// <summary>
        /// Starts the client process.
        /// </summary>
        /// <exception cref="System.Exception">Missing configuration file or wrong directory</exception>
        public void StartClientProcess()
        {
            if (!File.Exists(ConfigurationFilePath))
            {
                throw new Exception("Missing configuration file or wrong directory");
            }
            this.ElementConfigurator = new ElementConfigurator(this.ConfigurationFilePath);
            this.ClientNode = ElementConfigurator.ConfigureNode();
            List<StreamData> streamData = new List<StreamData>();
            streamData.Add(new StreamData(0, NetworkNode.SDHFrame.StmLevel.STM1, NetworkNode.SDHFrame.VirtualContainerLevel.VC12, 0, 5));
            streamData.Add(new StreamData(0, NetworkNode.SDHFrame.StmLevel.STM1, NetworkNode.SDHFrame.VirtualContainerLevel.VC12, 0, 4));
            streamData.Add(new StreamData(0, NetworkNode.SDHFrame.StmLevel.STM1, NetworkNode.SDHFrame.VirtualContainerLevel.VC12, 0, 6));
            this.ClientNode.AddStreamData(streamData);
            if (this.ClientNode.GetStreamData() != null && this.ClientNode.GetStreamData().Count != 0)
            {
                if (StreamsCreated != null)
                    StreamsCreated();
            }
        }
    }
}
