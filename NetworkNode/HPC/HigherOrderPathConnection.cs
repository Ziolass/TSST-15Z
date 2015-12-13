using NetworkNode.Frame;
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
        
        object bufferLock = new object();

        public HigherOrderPathConnection(TransportTerminalFunction ttf)
        {
            this.forwardingTable = new Dictionary<int, List<ForwardingRecord>>();
            this.ttf = ttf;
            this.ttf.HandleInputFrame += new HandleInputFrame(handleIncomFrame);
            builder = new FrameBuilder();
        }



        public void AddForwardingRecord(ForwardingRecord record)
        {
            if (!forwardingTable.ContainsKey(record.InputPort))
            {
                forwardingTable.Add(record.InputPort, new List<ForwardingRecord>());
            }

            forwardingTable[record.InputPort].Add(record);
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
