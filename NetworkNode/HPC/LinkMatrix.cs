using NetworkNode.HPC.Exceptions;
using NetworkNode.SDHFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.HPC
{


    public class LinkOccupiedArgs : EventArgs
    {
        public List<int> Ports { get; set; }
        public LinkOccupiedArgs(List<int> ports)
        {
            Ports = ports;
        }
    }
    public class LinkFreeArgs : EventArgs
    {
        public List<int> Ports { get; set; }
        public LinkFreeArgs(List<int> ports)
        {
            Ports = ports;
        }
    }

    public class PortResources
    {
        public int Counter { get; private set; }
        public int MaxLength { get; private set; }
        private List<int> FreeIndexes;
        private object[] Resources;

        public PortResources(int maxLength)
        {
            MaxLength = maxLength;
            Resources = new object[maxLength];
            Counter = 0;
            FreeIndexes = new List<int>();

            for (int i = 0; i < maxLength; i++)
            {
                FreeIndexes.Add(i);
            }
        }

        public bool CheckAvlibility(int index)
        {
            return Resources.Length < index ? Resources[index] == null : false;
        }

        public void SetOccupied(int index)
        {
            if (!CheckAvlibility(index))
            {
                throw new Exception("Resources already allocated");
            }

            Counter++;
            Resources[index] = new object();
        }

        public void FreeOccupied(int index)
        {
            if (Resources[index] == null)
            {
                return;
            }
            Counter--;
            Resources[index] = null;
        }

        public int? SetNextAvailable()
        {
            int index = 0;
            foreach (object allocatedResource in Resources)
            {
                if (allocatedResource == null)
                {
                    Resources[index] = new object();
                    Counter++;
                    return index;
                }

                index++;
            }

            return null;
        }

        public bool IsNextAvailable()
        {
            return Counter < MaxLength;
        }

    }

    public delegate void LinkOccupied(object sender, LinkOccupiedArgs args);
    public delegate void LinkFree(object sender, LinkFreeArgs args);

    public class LinkMatrix
    {
        private int VcMultiplaier;
        Dictionary<int, PortResources> PortsResources;

        public event LinkOccupied LinkOccupied;
        public event LinkFree LinkFree;
        
        public object OccupationLock;

        public LinkMatrix(VirtualContainerLevel vcLevel)
        {
            VcMultiplaier = VirtualContainerLevelExt.GetContainersNumber(vcLevel);
            PortsResources = new Dictionary<int, PortResources>();
            OccupationLock = new object();
        }

        public void Inint(Dictionary<int, StmLevel> ports)
        {
            foreach (int portNumber in ports.Keys)
            {
                StmLevel level = ports[portNumber];
                int maxLinksNumber = StmLevelExt.GetHigherPathsNumber(level) * VcMultiplaier;
                PortResources resource = new PortResources(maxLinksNumber);
                PortsResources.Add(portNumber, resource);
            }
        }

        public void OccupyResources(ForwardingRecord record)
        {
            Dictionary<int, int> portIndexes = GetInexes(record);
            OccupyPorts(portIndexes);
        }

        public void FreeResources(ForwardingRecord record)
        {
            Dictionary<int, int> portIndexes = GetInexes(record);
            OccupyPorts(portIndexes);
        }

        public void FreePorts(Dictionary<int, int> ports)
        {
            lock (OccupationLock)
            {
                if (ports.Count != 2)
                {
                    throw new Exception("API Interface error : FreePorts");
                }
                List<int> free = new List<int>();

                foreach (int portNumber in ports.Keys)
                {
                    int occupiedIndex = ports[portNumber];

                    if (PortsResources[portNumber].Counter == PortsResources[portNumber].MaxLength)
                    {
                        free.Add(portNumber);
                    }

                    PortsResources[portNumber].FreeOccupied(occupiedIndex);
                }

                if (free.Count > 0 && LinkOccupied != null)
                {
                    LinkFree(this, new LinkFreeArgs(free));
                }
            }
        }

        public Dictionary<int, int> OccupyPorts(Dictionary<int, int> ports)
        {
            lock (OccupationLock)
            {
                List<int> exhausted = new List<int>();

                foreach (int portNumber in ports.Keys)
                {
                    int occupiedIndex = ports[portNumber];

                    PortsResources[portNumber].SetOccupied(occupiedIndex);//TO może powodować błędy
                    
                    if (PortsResources[portNumber].Counter == PortsResources[portNumber].MaxLength)
                    {
                        exhausted.Add(portNumber);
                    }
                }

                if (exhausted.Count > 0 && LinkOccupied != null)
                {
                    LinkOccupied(this, new LinkOccupiedArgs(exhausted));
                }
            }
            return ports;
        }

        public Dictionary<int, int> OccupyNextAvalible(List<int> ports)
        {
            lock (OccupationLock)
            {
                Dictionary<int, int> result = new Dictionary<int, int>();
                foreach (int port in ports)
                {
                    if (!PortsResources[port].IsNextAvailable())
                    {
                        throw new AllLinksOccupied();
                    }
                }
                
                List<int> exhausted = new List<int>();
                
                foreach (int port in ports)
                {
                    int? portResourceIndex = PortsResources[port].SetNextAvailable();
                    result.Add(port, portResourceIndex.Value);
                    
                    if (PortsResources[port].Counter == PortsResources[port].MaxLength)
                    {
                        exhausted.Add(port);
                    }
                }

                if (exhausted.Count > 0 && LinkOccupied != null)
                {
                    LinkOccupied(this, new LinkOccupiedArgs(exhausted));
                }

                return result;

            }
        }

        private Dictionary<int, int> GetInexes(ForwardingRecord record)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            result.Add(record.InputPort, record.HigherPathIn * VcMultiplaier + record.VcNumberIn);
            result.Add(record.OutputPort, record.HigherPathOut * VcMultiplaier + record.VcNumberOut);
            return result;
        }

    }
}
