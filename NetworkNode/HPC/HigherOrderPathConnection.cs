using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNode.HPC
{
    
    public class HigherOrderPathConnection
    {
        private Dictionary<int, List<ForwardingRecord>> forwardingTable;
        private IFrameBuilder builder;
        private TransportTerminalFunction ttf;
        private Dictionary<int, IFrame> portsCredentials;

        object bufferLock = new object();

        public HigherOrderPathConnection(TransportTerminalFunction ttf)
        {
            this.forwardingTable = new Dictionary<int, List<ForwardingRecord>>();
            this.ttf = ttf;
            this.ttf.HandleInputFrame += new HandleInputFrame(handleIncomFrame);
            builder = new FrameBuilder();
            portsCredentials = new Dictionary<int, IFrame>();
            List<int> allPorts = new List<int>();
            foreach (List<int> ports in ttf.GetPorts())
            {
                allPorts.AddRange(ports);
            }

            foreach (int port in allPorts)
            {
                portsCredentials.Add(port, new Frame());
            }
        }



        public ExecutionResult AddForwardingRecords(List<ForwardingRecord> records)
        {
            int index = 0;
            foreach (ForwardingRecord record in records)
            {
                
                if (!checkForwardingRecord(record))
                {
                    return new ExecutionResult(false,"Error at record " + index);
                }
                index++;
            }

            foreach(ForwardingRecord record in records) 
            {
                if (!forwardingTable.ContainsKey(record.InputPort))
                {
                    forwardingTable.Add(record.InputPort, new List<ForwardingRecord>());
                }

                forwardingTable[record.InputPort].Add(record);
            }

            return new ExecutionResult(true,null);
        }

        private bool checkForwardingRecord(ForwardingRecord record)
        {
            VirtualContainer vc = new VirtualContainer(record.ContainerLevel);
            return portsCredentials[record.InputPort].SetVirtualContainer(record.ContainerLevel, record.VcNumberIn, vc) &&
                portsCredentials[record.OutputPort].SetVirtualContainer(record.ContainerLevel, record.VcNumberIn, vc);
        }

        public List<ForwardingRecord> GetForwardingRecords()
        {
            List<ForwardingRecord> routerRecords = new List<ForwardingRecord>();
            foreach (List<ForwardingRecord> portRecords in forwardingTable.Values)
            {
                routerRecords.AddRange(portRecords);
            }

            return routerRecords;
        }

        private void handleIncomFrame(object sender, InputFrameArgs args)
        {
            IFrame bufferedFrame = args.Frame;
            Dictionary<int, IFrame> outputFrames = new Dictionary<int, IFrame>();
            commuteFrame(args.PortNumber, bufferedFrame, outputFrames);

            transportData(outputFrames);
        }

        private void commuteFrame(int input, IFrame frame, Dictionary<int, IFrame> outputFrames)
        {
            List<ForwardingRecord> forwardingRules = forwardingTable[input];

            foreach (ForwardingRecord record in forwardingRules)
            {
                if (!outputFrames.ContainsKey(record.OutputPort))
                {
                    outputFrames.Add(record.OutputPort, builder.BuildEmptyFrame());
                }

                IFrame outputFrame = outputFrames[record.OutputPort];
                IContent vContainer = frame.GetVirtualContainer(record.ContainerLevel, record.VcNumberIn);

                outputFrame.SetVirtualContainer(record.ContainerLevel, record.VcNumberOut, vContainer);
            }
        }

        private void transportData(Dictionary<int, IFrame> outputFrames)
        {
            ttf.PassDataToInterfaces(outputFrames);
        }
    }
}
