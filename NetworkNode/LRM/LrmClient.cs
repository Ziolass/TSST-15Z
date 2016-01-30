using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM
{
    public enum LrmCommunicationType
    {
        DATA, SIGNALLING
    }

    public enum LrmHeader
    {
        INFO, BROADCAST
    }

    public delegate void LrmTokneAns(string ans);
    public class LrmClient
    {
        private LrmClientServer MsgClient;

        Action<string> BrodcastToken;
        Action<string> HandleLrmAction;

        public LrmClient(int msgClientPort,
            Action<string> brodcastToken,
            Action<string> handleLrmAction)
        {
            MsgClient = new LrmClientServer(msgClientPort, DispatchCommunication);
            BrodcastToken = brodcastToken;
            HandleLrmAction = handleLrmAction;
        }

        public void DispatchCommunication(string inputData)
        {
            string[] dataWithProtocol = inputData.Split('|');
            string protocol = dataWithProtocol[0];
            string data = dataWithProtocol[1];
            LrmCommunicationType type = (LrmCommunicationType)Enum.Parse(typeof(LrmCommunicationType), protocol);

            switch (type)
            {
                case LrmCommunicationType.SIGNALLING:
                    {
                        BrodcastToken(data);
                        break;
                    }
                case LrmCommunicationType.DATA:
                    {
                        HandleLrmAction(data);
                        break;
                    }
            }
        }

        private void HandleSignalling(string signallingPacket)
        {
            string[] signallingPacketParted = signallingPacket.Split('#');
            string signallingType = signallingPacketParted[0];
            string data = signallingPacketParted[0];
            LrmHeader type = (LrmHeader)Enum.Parse(typeof(LrmHeader), signallingType);
            switch (type)
            {
                case LrmHeader.INFO:
                    {

                        Console.WriteLine(data);
                        break;
                    }
                case LrmHeader.BROADCAST:
                    {
                        BrodcastToken(data);
                        break;
                    }
                
            }

        }

        public void ReportToken(LrmToken token)
        {
            string lrmMessage = JsonConvert.SerializeObject(token);
            MsgClient.SendToLrm(lrmMessage);
        }

        public void SendLrmHandshake()
        {
            SimpleLrmMsg lrmHandshake = new SimpleLrmMsg()
            {
                Msg = "Hello"
            };

            string lrmMessage = JsonConvert.SerializeObject(lrmHandshake);
            MsgClient.SendToLrm(lrmMessage);
        }
    }
}
