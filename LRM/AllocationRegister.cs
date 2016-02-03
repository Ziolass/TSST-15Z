using LRM.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRM
{
    public class AllocationRegister
    {
        private Dictionary<string, Dictionary<string, bool>> AllocationLog;
        private Dictionary<string, AsyncCommunication> CcCommunication;
        private Dictionary<string, ConnectionRequest> Connections;
        public AllocationRegister()
        {
            AllocationLog = new Dictionary<string, Dictionary<string, bool>>();
            CcCommunication = new Dictionary<string, AsyncCommunication>();
            Connections = new Dictionary<string, ConnectionRequest>();
        }

        public void AddConnection(string connectionName, AsyncCommunication async, ConnectionRequest connection)
        {
            AllocationLog.Add(connectionName, new Dictionary<string,bool>());
            CcCommunication.Add(connectionName, async);
            Connections.Add(connectionName, connection);
        }

        public void RegisterStep(string connectionName, string stepId)
        {
            AllocationLog[connectionName].Add(stepId, false);
        }

        public bool ConfirmStep(string connectionName, string stepId)
        {
            AllocationLog[connectionName][stepId] = true;
            return Array.TrueForAll(AllocationLog[connectionName].Values.ToArray(), x => x);
        }
        public AsyncCommunication GetComm(string connectionName)
        {
            return CcCommunication[connectionName];
        }

        public ConnectionRequest GetConnection(string connectionName)
        {
            return Connections[connectionName];
        }

        public void Remove(string connectionName)
        {
            AllocationLog.Remove(connectionName);
            CcCommunication.Remove(connectionName);
            Connections.Remove(connectionName);
        }
    }
}
