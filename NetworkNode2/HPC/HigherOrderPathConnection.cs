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
    public delegate void LinkResourceAlloc(object sender, List<LrmPort> args);
    public delegate void LinkResourceDelloc(object sender, List<LrmPort> args);

    public class HigherOrderPathConnection
    {
        private Dictionary<int, List<ForwardingRecord>> ForwardingTable;
        private List<ForwardingRecord> Connections;
        private IFrameBuilder Builder;
        //private LinkMatrix LinkResources;
        public VirtualContainerLevel NetworkDefaultLevel { get; private set; }

        private TransportTerminalFunction Ttf;
        //Credentials [Redundant but Faster]
        private Dictionary<int, IFrame> InputCredentials;
        private Dictionary<int, IFrame> OutputCredentials;

        public event HandleLrmInfo HandleLrmInfo;
        public event LinkResourceAlloc LinkResourceAlloc;
        public event LinkResourceDelloc LinkResourceDelloc;

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

            Dictionary<int, StmLevel> allPorts = ttf.GetPorts();

            foreach (int port in allPorts.Keys)
            {
                InputCredentials.Add(port, new Frame(allPorts[port]));
                OutputCredentials.Add(port, new Frame(allPorts[port]));
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
                    bool inputOcp = InputCredentials[record.InputPort].IsFrameOccupied(NetworkDefaultLevel);

                    if (!CheckForwardingRecord(record))
                    {
                        foreach (ForwardingRecord checkedRecord in checkedRecords)
                        {
                            ClearCredentials(checkedRecord);
                        }

                        return new ExecutionResult
                        {
                            Msg = "Error at record " + index,
                            Ports = null,
                            Result = false
                        };
                    }

                    if (!inputOcp &&
                        !portOccupataion.Contains(record.InputPort) &&
                        InputCredentials[record.InputPort].IsFrameOccupied(NetworkDefaultLevel))
                    {
                        portOccupataion.Add(record.InputPort);
                    }

                    checkedRecords.Add(record);
                    index++;
                }

                Connections.Add(twoWayRecord[0]);

                if (!resourceLocation)
                {
                    List<LrmPort> ports = GetInexes(twoWayRecord[0]);
                    
                    if (LinkResourceAlloc != null)
                    {
                        LinkResourceAlloc(this, ports);
                    }

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

            return new ExecutionResult
            {
                Msg = null,
                Ports = portOccupataion,
                Result = true
            };
        }

        public ExecutionResult Allocate(List<LrmPort> ports)
        {
            List<ForwardingRecord> twoWayRecord = TransformToTwoWayRecord(ports);

            List<List<ForwardingRecord>> records = new List<List<ForwardingRecord>>();
            records.Add(twoWayRecord);

            try
            {
                return AddForwardingRecords(records, false);
            }
            catch (Exception ex)
            {
                return new ExecutionResult
                {
                    Result = false,
                    Msg = ex.Message
                };
            }
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
                    new KeyValuePair<int, int>(ports[0], data[ports[0]]),
                    level
                );

            twoWayRecord.Add(fwr1);
            twoWayRecord.Add(fwr2);
            return twoWayRecord;
        }

        public ExecutionResult FreeResources(List<LrmPort> ports)
        {
            List<ForwardingRecord> twoWayRecord = TransformToTwoWayRecord(ports);
            return RemoveTwWayRecord(twoWayRecord, true);
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

        public ExecutionResult RemoveTwWayRecord(List<ForwardingRecord> records, bool resourceAllocation)
        {
            if (records.Count != 2)
            {
                return new ExecutionResult
                {
                    Msg = "Attempt to delete one way of connection",
                    Result = false
                };
            }

            foreach (ForwardingRecord record in records)
            {
                RemoveRecord(record);
            }

            return DeleteFromConnections(records, resourceAllocation);
        }

        private ExecutionResult DeleteFromConnections(List<ForwardingRecord> records, bool resourceAction)
        {
            foreach (ForwardingRecord record in records)
            {
                if (Connections.Contains(record))
                {
                    Connections.Remove(record);

                    if (!resourceAction)
                    {
                        List<LrmPort> ports = GetInexes(record);
                        if (LinkResourceDelloc != null)
                        {
                            LinkResourceDelloc(this, ports);
                        }
                    }

                    return new ExecutionResult
                    {
                        Result = true
                    };
                }
            }

            return new ExecutionResult
            {
                Result = false
            };
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

        private List<LrmPort> GetInexes(ForwardingRecord record)
        {
            int VcMultiplaier = VirtualContainerLevelExt.GetContainersNumber(NetworkDefaultLevel);
            List<LrmPort> result = new List<LrmPort>();
            result.Add(new LrmPort {
                Number = record.InputPort.ToString(), 
                Index = (record.HigherPathIn * VcMultiplaier + record.VcNumberIn).ToString()
            });
            result.Add(new LrmPort
            {
                Number = record.OutputPort.ToString(),
                Index = (record.HigherPathOut * VcMultiplaier + record.VcNumberOut).ToString()
            });
            return result;
        }

        private ExecutionResult ClearCredentials(ForwardingRecord record)
        {
            Frame inputCredential = (Frame)InputCredentials[record.InputPort];
            Frame outputCredential = (Frame)OutputCredentials[record.OutputPort];

            bool inputOccp = inputCredential.IsFrameOccupied(NetworkDefaultLevel);
            bool outputOccp = outputCredential.IsFrameOccupied(NetworkDefaultLevel);

            bool result = true;
            result = result && inputCredential.ClearVirtualContainer(record.ContainerLevel, record.HigherPathIn, record.VcNumberIn);
            result = result && outputCredential.ClearVirtualContainer(record.ContainerLevel, record.HigherPathOut, record.VcNumberOut);

            List<int> ports = new List<int>();

            if (!inputCredential.IsFrameOccupied(NetworkDefaultLevel) && inputOccp)
            {
                ports.Add(record.InputPort);
            }

            if (!outputCredential.IsFrameOccupied(NetworkDefaultLevel) && outputOccp)
            {
                ports.Add(record.OutputPort);
            }

            return new ExecutionResult
            {
                Ports = ports,
                Result = result
            };
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


        private ExecutionResult RemoveRecord(ForwardingRecord record)
        {
            if (!ForwardingTable.ContainsKey(record.InputPort))
            {
                return null;
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
                //To chyba chcia³em sprawdzaæ czy siê usunie najpierw
                ForwardingTable[record.InputPort].Remove(toRemove);
                return ClearCredentials(toRemove);
            }

            return new ExecutionResult
                {
                    Result = false
                };
        }
    }
}
