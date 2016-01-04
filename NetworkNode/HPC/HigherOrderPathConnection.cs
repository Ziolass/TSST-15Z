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
        private Dictionary<int, List<ForwardingRecord>> ForwardingTable;
        private List<ForwardingRecord> Connections;
        private IFrameBuilder builder;
        private TransportTerminalFunction ttf;
        private Dictionary<int, IFrame> InputCredentials;
        private Dictionary<int, IFrame> OutputCredentials;

        object bufferLock = new object();

        public HigherOrderPathConnection(TransportTerminalFunction ttf)
        {
            this.ForwardingTable = new Dictionary<int, List<ForwardingRecord>>();
            this.ttf = ttf;
            this.ttf.HandleInputFrame += new HandleInputFrame(handleIncomFrame);
            builder = new FrameBuilder();
            InputCredentials = new Dictionary<int, IFrame>();
            OutputCredentials = new Dictionary<int, IFrame>();
            Connections = new List<ForwardingRecord>();

            Dictionary<int,StmLevel> allPorts = ttf.GetPorts();
            
            foreach (int port in allPorts.Keys)
            {
                InputCredentials.Add(port, new Frame(allPorts[port]));
                OutputCredentials.Add(port, new Frame(allPorts[port]));
            }
        }

        public ExecutionResult AddForwardingRecords(List<List<ForwardingRecord>> records)
        {
            int index = 0;
            List<ForwardingRecord> checkedRecords = new List<ForwardingRecord>();
            foreach (List<ForwardingRecord> twoWayRecord in records)
            {
                
                foreach (ForwardingRecord record in twoWayRecord)
                {
                    if (!CheckForwardingRecord(record))
                    {
                        foreach (ForwardingRecord checkedRecord in checkedRecords)
                        {
                            ClearCredentials(checkedRecord);
                        }
                        return new ExecutionResult(false, "Error at record " + index);
                    }
                    checkedRecords.Add(record);
                    index++;
                }
                Connections.Add(twoWayRecord[0]);
            }

            foreach (ForwardingRecord record in checkedRecords) 
            {
                if (!ForwardingTable.ContainsKey(record.InputPort))
                {
                    ForwardingTable.Add(record.InputPort, new List<ForwardingRecord>());
                }

                ForwardingTable[record.InputPort].Add(record);
            }

            return new ExecutionResult(true,null);
        }

        private bool CheckForwardingRecord(ForwardingRecord record)
        {
            VirtualContainer vc = new VirtualContainer(record.ContainerLevel);

            if (InputCredentials[record.InputPort].SetVirtualContainer(record.ContainerLevel, record.HigherPathIn, record.VcNumberIn == -1 ? null : (int?)record.VcNumberIn, vc))
            {
                if (OutputCredentials[record.OutputPort].SetVirtualContainer(record.ContainerLevel, record.HigherPathOut, record.VcNumberOut == -1 ? null : (int?)record.VcNumberOut, vc))
                {
                    return true;
                }

                ((Frame)InputCredentials[record.InputPort]).ClearVirtualContainer(record.ContainerLevel, record.HigherPathIn, record.VcNumberIn == -1 ? null : (int?)record.VcNumberIn);
            } 

            return false;
        }

        private bool ClearCredentials(ForwardingRecord record)
        {
            Frame inputCredential = (Frame)InputCredentials[record.InputPort];
            Frame outputCredential = (Frame)OutputCredentials[record.OutputPort];
            bool result = true;
            result = result && inputCredential.ClearVirtualContainer(record.ContainerLevel, record.HigherPathIn, record.VcNumberIn);
            result = result && outputCredential.ClearVirtualContainer(record.ContainerLevel, record.HigherPathOut, record.VcNumberOut);
            return result;
        }

        public List<ForwardingRecord> GetConnections()
        {
            /*List<ForwardingRecord> routerRecords = new List<ForwardingRecord>();
            foreach (List<ForwardingRecord> portRecords in ForwardingTable.Values)
            {
                routerRecords.AddRange(portRecords);
            }*/

            return Connections;
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
            if (!ForwardingTable.ContainsKey(input))
            {
                return;
            }

            List<ForwardingRecord> forwardingRules = ForwardingTable[input];

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

        public bool RemoveTwWayRecord(List<ForwardingRecord> records) 
        {
            if (records.Count != 2)
            {
                return false;
            }

            foreach (ForwardingRecord record in records)
            {
                RemoveRecord(record);
            }

            if(Connections.Contains(records[0]))
            {
                Connections.Remove(records[0]);
                return true;
            }
            
            if (Connections.Contains(records[1]))
            {
                Connections.Remove(records[1]);
                return true;
            }
            return false;

        }
        private bool RemoveRecord(ForwardingRecord record)
        {
            if(!ForwardingTable.ContainsKey(record.InputPort)) 
            {
                return false;
            }
            List<ForwardingRecord> scope = ForwardingTable[record.InputPort];
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
               ClearCredentials(toRemove);
               ForwardingTable[record.InputPort].Remove(toRemove);
               return true;
            }
           
            return false;
        }
    }
}
