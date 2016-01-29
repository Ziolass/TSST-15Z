using NetworkNode.LRM.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.LRM
{
    public delegate void LrmTokneAns(string ans);
    public class LrmClient
    {
        private LrmClientServer MsgClient;

        Action<string> BrodcastToken;

        public LrmClient(int msgClientPort, Action<string> brodcastToken)
        {
            MsgClient = new LrmClientServer(msgClientPort);
            BrodcastToken = brodcastToken;
        }



        public void ReportToken(LrmToken token)
        {
            string lrmMessage = JsonConvert.SerializeObject(token); 
            MsgClient.SendMessage(lrmMessage, (string ans) =>
             {
                 TokenAns tokenAns = JsonConvert.DeserializeObject<TokenAns>(ans);
                 if (tokenAns.Status.Equals("OK"))
                 {
                     Console.WriteLine("Token Accepted");
                 }
                 else
                 {
                     Console.WriteLine("Token Cannot be accepted: " + lrmMessage);
                 }
             });
        }

        public void SendLrmHandshake()
        {
            SimpleLrmMsg lrmHandshake = new SimpleLrmMsg(){
                Msg = "Hello"
            };

            string lrmMessage = JsonConvert.SerializeObject(lrmHandshake);
            MsgClient.SendMessage(lrmMessage, BrodcastToken);
        }
    }
}
