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
        private Dictionary<int, IFrame> inputCredentials;
        private Dictionary<int, IFrame> outputCredentials;

        object bufferLock = new object();

        public HigherOrderPathConnection(TransportTerminalFunction ttf)
        {
            this.forwardingTable = new Dictionary<int, List<ForwardingRecord>>();
            this.ttf = ttf;
            this.ttf.HandleInputFrame += new HandleInputFrame(handleIncomFrame);
            builder = new FrameBuilder();
            inputCredentials = new Dictionary<int, IFrame>();
            outputCredentials = new Dictionary<int, IFrame>();
            List<int> allPorts = ttf.GetPorts();
            
            foreach (int port in allPorts)
            {
                inputCredentials.Add(port, new Frame());
                outputCredentials.Add(port, new Frame());
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
            
            /*if (inputCredentials[record.InputPort].SetVirtualContainer(record.ContainerLevel, record.VcNumberIn, vc))
            {
                if (outputCredentials[record.OutputPort].SetVirtualContainer(record.ContainerLevel, record.VcNumberIn, vc))
                {
                    return true;
                }

                ((Frame)inputCredentials[record.InputPort]).ClearVirtualContainer(record.ContainerLevel, record.VcNumberIn);
            } */

            return false;
        }

        private bool clearCredentials(ForwardingRecord record)
        {
            Frame inputCredential = (Frame)inputCredentials[record.InputPort];
            Frame outputCredential = (Frame)outputCredentials[record.InputPort];
            bool result = true;
            result = result && inputCredential.ClearVirtualContainer(record.ContainerLevel, record.VcNumberIn);
            result = result && outputCredential.ClearVirtualContainer(record.ContainerLevel, record.VcNumberOut);
            return result;
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
            if (!forwardingTable.ContainsKey(input))
            {
                return;
            }

            List<ForwardingRecord> forwardingRules = forwardingTable[input];

            foreach (ForwardingRecord record in forwardingRules)
            {
                if (!outputFrames.ContainsKey(record.OutputPort))
                {
                    outputFrames.Add(record.OutputPort, builder.BuildEmptyFrame());
                }

                IFrame outputFrame = outputFrames[record.OutputPort];
                IContent vContainer = frame.GetVirtualContainer(record.ContainerLevel, record.VcNumberIn);

                //outputFrame.SetVirtualContainer(record.ContainerLevel, record.VcNumberOut, vContainer);
            }
        }

        private void transportData(Dictionary<int, IFrame> outputFrames)
        {
            ttf.PassDataToInterfaces(outputFrames);
        }

        internal bool RemoveRecord(ForwardingRecord record)
        {
            if(!forwardingTable.ContainsKey(record.InputPort)) 
            {
                return false;
            }
            List<ForwardingRecord> scope = forwardingTable[record.InputPort];
            ForwardingRecord toRemove = null;
            foreach (ForwardingRecord scopeRecord in scope)
            {
                if (record.Equals(scopeRecord))
                {
                    toRemove = scopeRecord;
                    break;
                }
            }
            
            if (toRemove != null)
            {
               clearCredentials(toRemove);
               forwardingTable[record.InputPort].Remove(toRemove);
               return true;
            }
           
            return false;
        }
    }
}
