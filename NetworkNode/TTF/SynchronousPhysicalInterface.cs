
using NetworkNode.SDHFrame;
using NetworkNode.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireCloud;

namespace NetworkNode.TTF
{
    public class InputDataArgs : EventArgs
    {
        public int PortNumber { get; set; }
        public InputDataArgs(int portNumber)
        {
            PortNumber = portNumber;
        }
    }

    public delegate void HandleInputData(object sender, InputDataArgs args);

    public class SynchronousPhysicalInterface
    {
        private Dictionary<int, List<string>> Buffers;
        private NetworkNodeSender Sender;
        private Dictionary<int, NodeInput> Ports;
        private string RouterId;
        public String ReceivedFrame { get; private set; }
        public event HandleInputData HandleInputData;

        public SynchronousPhysicalInterface(List<NodeInput> ports, NetworkNodeSender sender, string routerId)
        {
            RouterId = routerId;
            Sender = sender;
            Buffers = new Dictionary<int, List<string>>();
            this.Ports = new Dictionary<int, NodeInput>();
            foreach (NodeInput input in ports)
            {
                Buffers.Add(input.InputPort, new List<string>());
                this.Ports.Add(input.InputPort, input);
                input.HandleIncomingData += new HandleIncomingData(pullData);
            }
        }



        private void pullData(object sender, EventArgs args)
        {
            NodeInput source = (NodeInput)sender;
            StringBuilder translator = new StringBuilder();
            byte[] data = source.GetDataFromBuffer();
            translator.Append(Encoding.ASCII.GetString(data, 0, data.Length));
            Buffers[source.InputPort].Add(translator.ToString());
            if (HandleInputData != null)
            {
                HandleInputData(this, new InputDataArgs(source.InputPort));
            }

        }

        public void SendFrame(String sdhFrame, int port)
        {
            if (!Ports.ContainsKey(port) || !Ports[port].Active)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(RouterId);
            builder.Append("|");
            builder.Append(port);
            builder.Append("|");
            builder.Append(sdhFrame);
            Sender.SendContent(builder.ToString());
        }
        //TODO: zweryfikować czy faktycznie usuniemy poprawnie dane z buffora
        public string GetBufferedData(int inputPort)
        {
            string result;
            List<string> bufferedData = Buffers[inputPort];

            if (bufferedData.Count > 0)
            {
                result = bufferedData[0];
                bufferedData.RemoveAt(0);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public bool ShudownInterface(int number)
        {
            try
            {
                if (Ports.ContainsKey(number))
                {
                    Ports[number].Active = false;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Dictionary<int,StmLevel> GetPorts()
        {
            Dictionary<int,StmLevel> result = new Dictionary<int,StmLevel>();
            foreach (int port in Ports.Keys)
            {
                result.Add(port,Ports[port].Level);
            }
            return result;
        }
    }
}
