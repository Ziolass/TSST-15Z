using NetworkNode.LRM.Communication;
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
    public delegate void HandleLrmInfo(object sender, InputLrmArgs args);
    public delegate void LinkResourceExhausted(object sender, LinkOccupiedArgs args);
    public delegate void LinkResourceFreed(object sender, LinkFreeArgs args);

    public class HigherOrderPathConnection
    {
        private Dictionary<int, List<ForwardingRecord>> ForwardingTable;
        private List<ForwardingRecord> Connections;
        private IFrameBuilder Builder;
        private LinkMatrix LinkResources;
        private VirtualContainerLevel NetworkDefaultLevel;

        private TransportTerminalFunction Ttf;
        //Credentials [Redundant but Faster]
        private Dictionary<int, IFrame> InputCredentials;
        private Dictionary<int, IFrame> OutputCredentials;

        public event HandleLrmInfo HandleLrmInfo;
        public event LinkResourceExhausted LinkResourceExhausted;
        public event LinkResourceFreed LinkResourceFreed;

        object bufferLock = new object();

        public HigherOrderPathConnection(TransportTerminalFunction ttf, VirtualContainerLevel networkDefaultLevel)
        {
            NetworkDefaultLevel = networkDefaultLevel;
            ForwardingTable = new Dictionary<int, List<ForwardingRecord>>();
            Ttf = ttf;
            Ttf.HandleInputFrame += new HandleInputFrame(HandleIncomFrame);
            Ttf.HandleLrmData += new HandleLrmData(HandleLrm);

            Builder = new FrameBuilder();
            InputCredentials = new Dictionary<int, IFrame>();
            OutputCredentials = new Dictionary<int, IFrame>();
            Connections = new List<ForwardingRecord>();
            //TODO may be configurable by injecting through constructor
            LinkResources = new LinkMatrix(VirtualContainerLevel.VC32);

            Dictionary<int, StmLevel> allPorts = ttf.GetPorts();
            LinkResources.Inint(allPorts);

            LinkResources.LinkFree += new LinkFree(HandleLinkResourceFreed);
            LinkResources.LinkOccupied += new LinkOccupied(HandleLinkResourceExhausted);

            foreach (int port in allPorts.Keys)
            {
                InputCredentials.Add(port, new Frame(allPorts[port]));
                OutputCredentials.Add(port, new Frame(allPorts[port]));
            }
        }

        public void HandleLinkResourceExhausted(object sender, LinkOccupiedArgs args)
        {
            if (LinkResourceExhausted != null)
            {
                LinkResourceExhausted(this, args);
            }
        }

        public void HandleLinkResourceFreed(object sender, LinkFreeArgs args)
        {
            if (LinkResourceFreed != null)
            {
                LinkResourceFreed(this, args);
            }
        }

        public ExecutionResult AddForwardingRecords(List<List<ForwardingRecord>> records, bool resourceLocation)
        {
            int index = 0;
            List<ForwardingRecord> checkedRecords = new List<ForwardingRecord>();
            List<int> portOccupataion = new List<int>();
            
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
                    //Asumption: two way record consisit of two one way records, in each, order of input and output 
                    //is different
                    if(InputCredentials[record.InputPort].)
                    checkedRecords.Add(record);
                    index++;
                }

                Connections.Add(twoWayRecord[0]);

                if (resourceLocation)
                {
                    LinkResources.OccupyResources(twoWayRecord[0]);
                }

            }

            foreach (ForwardingRecord record in checkedRecords)
            {
                if (!ForwardingTable.ContainsKey(record.InputPort))
                {
                    ForwardingTable.Add(record.InputPort, new List<ForwardingRecord>());
                }

                ForwardingTable[record.InputPort].Add(record);
            }

            return new ExecutionResult(true, null);
        }

        public bool Allocate(List<LrmPort> ports)
        {
            bool result;
            List<ForwardingRecord> twoWayRecord = TransformToTwoWayRecord(ports);
            
            List<List<ForwardingRecord>> records = new List<List<ForwardingRecord>>();
            records.Add(twoWayRecord);

            try
            {
                result = AddForwardingRecords(records, false).Result;
                //
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private List<ForwardingRecord> TransformToTwoWayRecord(List<LrmPort> ports)
        {
            Dictionary<int, int> resources = new Dictionary<int, int>();

            foreach (LrmPort lrmPort in ports)
            {
                int index = int.Parse(lrmPort.Index);
                int number = int.Parse(lrmPort.Index);
                resources.Add(index, number);
            }

            return TranslateToRecords(resources, NetworkDefaultLevel);
        }

        private List<ForwardingRecord> TranslateToRecords(Dictionary<int, int> data, VirtualContainerLevel level)
        {
            List<int> ports = new List<int>(data.Keys);
           
            List<ForwardingRecord> twoWayRecord = new List<ForwardingRecord>();
            ForwardingRecord fwr1 =
                CreateForwardingRecord(
                    new KeyValuePair<int, int>(ports[0], data[ports[0]]),
                    new KeyValuePair<int, int>(ports[1], data[ports[1]]),
                    level
                );
            ForwardingRecord fwr2 =
                CreateForwardingRecord(
                    new KeyValuePair<int, int>(ports[1], data[ports[1]]),
                    new KeyValuePair<int, int>(ports[2], data[ports[2]]),
                    level
                );

            twoWayRecord.Add(fwr1);
            twoWayRecord.Add(fwr2);
            return twoWayRecord;
        }

        public void FreeResources(Dictionary<int, int> data)
        {
            List<ForwardingRecord> twoWayRecord = TranslateToRecords(data, VirtualContainerLevel.VC32);
            RemoveTwWayRecord(twoWayRecord);
        }

        private ForwardingRecord CreateForwardingRecord(KeyValuePair<int, int> srcResources, KeyValuePair<int, int> dstResources, VirtualContainerLevel level)
        {
            return new ForwardingRecord(srcResources.Key,
                dstResources.Key,
                level,
                srcResources.Value % 3,
                dstResources.Value % 3,
                (int)(srcResources.Value / 3),
                (int)(dstResources.Value / 3));
        }

        public List<ForwardingRecord> GetConnections()
        {
            return Connections;
        }

        public void SendLrmData(int port, string data)
        {
            Ttf.SendLrmData(port, data);
        }

        public List<int> GetPortsForLrm()
        {
            return new List<int>(Ttf.GetPorts().Keys);
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

            if (Connections.Contains(records[0]))
            {
                LinkResources.FreeResources(records[0]);
                Connections.Remove(records[0]);
                return true;
            }

            if (Connections.Contains(records[1]))
            {
                LinkResources.FreeResources(records[0]);
                Connections.Remove(records[1]);
                return true;
            }

            return false;

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

        private void HandleIncomFrame(object sender, InputFrameArgs args)
        {
            IFrame bufferedFrame = args.Frame;
            Dictionary<int, IFrame> outputFrames = new Dictionary<int, IFrame>();
            CommuteFrame(args.PortNumber, bufferedFrame, outputFrames);

            transportData(outputFrames);
        }

        private void HandleLrm(object sender, InputLrmArgs args)
        {
            if (HandleLrmInfo != null)
            {
                HandleLrmInfo(this, args);
            }
        }

        private void CommuteFrame(int input, IFrame frame, Dictionary<int, IFrame> outputFrames)
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
                    Dictionary<int, StmLevel> portStmLevels = this.Ttf.GetPorts();
                    StmLevel outputFrameLevel = portStmLevels[record.OutputPort];
                    outputFrames.Add(record.OutputPort, new Frame(outputFrameLevel));
                }

                IFrame outputFrame = outputFrames[record.OutputPort];
                IContent vContainer = frame.GetVirtualContainer(record.ContainerLevel, record.HigherPathIn, record.VcNumberIn == -1 ? null : (int?)record.VcNumberIn);

                outputFrame.SetVirtualContainer(record.ContainerLevel, record.HigherPathOut, record.VcNumberOut, vContainer);
            }
        }

        private void transportData(Dictionary<int, IFrame> outputFrames)
        {
            Ttf.PassDataToInterfaces(outputFrames);
        }


        private bool RemoveRecord(ForwardingRecord record)
        {
            if (!ForwardingTable.ContainsKey(record.InputPort))
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
