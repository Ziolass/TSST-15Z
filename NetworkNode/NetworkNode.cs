using NetworkNode.HPC;
using NetworkNode.MenagmentModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    public class NetworkNode
    {
        private ManagementCenter managementCenter;
        private HigherOrderPathConnection hpc;
        public NetworkNode(ManagementCenter managementCenter, HigherOrderPathConnection hpc)
        {
            this.managementCenter = managementCenter;
            this.hpc = hpc;
        }

        public void emulateManagement(String request)
        {
            managementCenter.PerformManagementAction(request);
        }
    }
}
