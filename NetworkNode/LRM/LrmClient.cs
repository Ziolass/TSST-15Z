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
        private LrmClientSender LrmCommunication;

        Action<string> BrodcastToken;
        Action<string> HandleLrmAction;

        public LrmClient(int msgClientPort,
            Action<string> brodcastToken,
            Action<string> handleLrmAction)
        {
            LrmCommunication = new LrmClientSender(msgClientPort, DispatchCommunication);
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
                        HandleSignalling(data);
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
                        Console.WriteLine("LRM INFO: {0}", data);
                        break;
                    }
                case LrmHeader.BROADCAST:
                    {
                        BrodcastToken(data);
                        break;
                    }
                
            }

        }

        public void SendLrmMessage(LrmToken token)
        {
            string lrmMessage = JsonConvert.SerializeObject(token);
            LrmCommunication.SendToLrm(lrmMessage);
        }

        public void SendLrmMessage(LrmResp resp)
        {
            string lrmMessage = JsonConvert.SerializeObject(resp);
            LrmCommunication.SendToLrm(lrmMessage);
        }

        public void SendLrmHandshake()
        {
            SimpleLrmMsg lrmHandshake = new SimpleLrmMsg()
            {
                Msg = "Hello"
            };

            string lrmMessage = JsonConvert.SerializeObject(lrmHandshake);
            LrmCommunication.SendToLrm(lrmMessage);
        }
    }
}
