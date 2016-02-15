using NetworkNode.LRM;
using NetworkNode.LRM.Communication;
using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode.Adaptation
{
    public class ClientData : EventArgs 
    {
        public Dictionary<StreamData,string> StreamsData {get;set;}
        public ClientData (Dictionary<StreamData,string> streamsData) 
        {
            StreamsData = streamsData;
        }
        public string ToString()
        {
            string returnString = string.Empty;

            foreach (var item in StreamsData)
            {
                returnString += item.Value + "\n";
            }
            return returnString;
        }
    }
    public delegate void HandleClientData(ClientData data);
    public delegate void AllocationClientStream(List<StreamData> streams);
    public delegate void DeallocationClientStream(List<StreamData> streams);
    public class AdaptationFunction
    {
        private TransportTerminalFunction Ttf;
        private List<StreamData> Streams;
        private Dictionary<int, IFrame> OutputCredentials;
        private FrameBuilder Builder;
        private String routerId;
        private LrmIntroduce LrmIntroduce;
        private LrmClient LrmClient;
        private VirtualContainerLevel NetworkDefaultLevel;

        public event HandleClientData HandleClientData;
        public event AllocationClientStream AllocationClientStream;
        public event DeallocationClientStream DeallocationClientStream;
        public AdaptationFunction(TransportTerminalFunction ttf, 
            LrmIntroduce lrmIntroduce, 
            int lrmPort,
            VirtualContainerLevel networkDefaultLevel)
        {
            NetworkDefaultLevel = networkDefaultLevel;
            Ttf = ttf;
            Builder = new FrameBuilder();
            Ttf.HandleInputFrame += new HandleInputFrame(GetDataFromFrame);
            Ttf.HandleLrmData += new HandleLrmData(ReportLrmToken);
            LrmClient = new LrmClient(lrmPort, SendLrmToken, HandleLrmResourceManagement);
            LrmIntroduce = lrmIntroduce;
            routerId = LrmIntroduce.Node;
            this.Streams = new List<StreamData>();

            Dictionary<int, StmLevel> portsLevels = Ttf.GetPorts();
            OutputCredentials = new Dictionary<int, IFrame>();
            
            foreach (int portNumber in portsLevels.Keys)
            {
                OutputCredentials.Add(portNumber, new Frame(portsLevels[portNumber]));
            }

        }

        public void GetDataFromFrame(object sender, InputFrameArgs args)
        {
            int inputPort = args.PortNumber;
            List<StreamData> streamForPort = new List<StreamData>();
            Dictionary<StreamData, string> streamsData = new Dictionary<StreamData, string>();
            foreach (StreamData stream in Streams)
            {
                if (stream.Port == inputPort)
                {
                    string content = null;

                    VirtualContainer vc = (VirtualContainer)args.Frame.GetVirtualContainer(stream.VcLevel, stream.HigherPath, stream.LowerPath);
                    if (vc != null)
                    {
                        Container conteriner = vc.Content.Count > 0 ? vc.Content[0] as Container : null;
                        if (conteriner != null)
                        {
                            content = conteriner.Content;
                            if (content == null || content.Equals(""))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            content = "Error: Container does not transport client data";
                        }
                    }
                    else
                    {
                        content = "Error: Container does not transport client data";
                    }

                    streamsData.Add(stream, content);
                }
            }

            if (HandleClientData != null)
            {
                HandleClientData(new ClientData(streamsData));
            }
        }
        public void SentData(Dictionary<StreamData,string> dataToSent)
        {
            Dictionary<int, IFrame> outputData = new Dictionary<int, IFrame>();
            foreach (StreamData stream in dataToSent.Keys)
            {
                if (!Streams.Contains(stream))
                {
                    //TODO można rzucać wyjątek
                    continue;
                }

                if (!outputData.ContainsKey(stream.Port))
                {
                    outputData.Add(stream.Port, new Frame(stream.Stm));
                }

                //TODO ewentualne dzielenie danych
                Container content = new Container(dataToSent[stream]);
                VirtualContainer vc = new VirtualContainer(stream.VcLevel, content);
                outputData[stream.Port].SetVirtualContainer(stream.VcLevel, stream.HigherPath, stream.LowerPath, vc);
            }
            
            Ttf.PassDataToInterfaces(outputData);        
        }

        public ExecutionResult AddStreamData(List<StreamData> records)
        {
            int index = 0;
            List<int> avaliblePorts = new List<int>();
            foreach (StreamData record in records)
            {
                bool outputOccp = ((Frame)OutputCredentials[record.Port]).IsFrameOccupied(NetworkDefaultLevel);
                
                if (!CheckStreamData(record))
                {
                    return new ExecutionResult
                    {
                        Result = false,
                        Msg = "Error at record " + index
                    };
                }

                if (!outputOccp && ((Frame)OutputCredentials[record.Port]).IsFrameOccupied(NetworkDefaultLevel))
                {
                    avaliblePorts.Add(record.Port);
                }
                index++;
            }

            Streams.AddRange(records);

            return new ExecutionResult
            {
                Result = true,
                Msg = null,
                Ports = avaliblePorts
            };
        }

        public void SendLrmToken(string lrmToken)
        {

            foreach (KeyValuePair<int, StmLevel> port in Ttf.GetPorts())
            {
                LrmToken token = JsonConvert.DeserializeObject<LrmToken>(lrmToken);
                int hPathNo = StmLevelExt.GetHigherPathsNumber(port.Value);
                int containers = VirtualContainerLevelExt.GetContainersNumber(NetworkDefaultLevel);

                token.SenderPort = port.Key.ToString();
                token.StmMaxIndex = ((hPathNo * containers) - 1).ToString();

                Ttf.SendLrmData(port.Key, JsonConvert.SerializeObject(token));
            }
        }

        private void HandleLrmResourceManagement(string data)
        {
            LrmReq request = JsonConvert.DeserializeObject<LrmReq>(data);

            string textualRequest = request.ReqType.ToString();
            ReqType reqType = (ReqType)Enum.Parse(typeof(ReqType), textualRequest);

            switch (reqType)
            {
                case ReqType.ALLOC:
                    {
                        Alloc(request);
                        break;
                    }
                case ReqType.DELLOC:
                    {
                        Delloc(request);
                        break;
                    }
            }

        }

        private void Alloc(LrmReq request)
        {
            List<StreamData> streamsToAdd = new List<StreamData>();
            foreach (LrmPort port in request.Ports)
            {
                streamsToAdd.Add(TransformLrmPort(port));
            }

            ExecutionResult allocationResult = AddStreamData(streamsToAdd);

            LrmResp resp = new LrmResp
            {
                Type = ReqType.ALLOC_RESP.ToString(),
                Status = allocationResult.Result ?
                    LrmRespStatus.ACK.ToString()
                    : LrmRespStatus.ERROR.ToString(),
                Id = request.Id,
                ConnectionId = request.ConnectionId
            };
            LrmClient.SendLrmMessage(resp);

            if (AllocationClientStream != null)
            {
                AllocationClientStream(new List<StreamData>(streamsToAdd));
            }
        }

        private void Delloc(LrmReq request)
        {
            List<StreamData> streamsToRemove = new List<StreamData>();
            foreach (LrmPort port in request.Ports)
            {
                streamsToRemove.Add(TransformLrmPort(port));
            }

            ExecutionResult delocationResult = RemoveStreamData(streamsToRemove);

            LrmResp resp = new LrmResp
            {
                Type = ReqType.DELLOC_RESP.ToString(),
                Status = delocationResult.Result ?
                    LrmRespStatus.ACK.ToString()
                    : LrmRespStatus.ERROR.ToString(),
                Id = request.Id,
                ConnectionId = request.ConnectionId
            };
            LrmClient.SendLrmMessage(resp);

            if (DeallocationClientStream != null)
            {
                DeallocationClientStream(new List<StreamData>(streamsToRemove));
            }
        }

        private StreamData TransformLrmPort(LrmPort port)
        {
            int portNumber = int.Parse(port.Number);
            int index = int.Parse(port.Index);
            return new StreamData {
                HigherPath = (int)(index / 3),
                LowerPath = index % 3,
                VcLevel = NetworkDefaultLevel,
                Port = portNumber,
                Stm = Ttf.GetPorts()[portNumber]
            };
        }

        private bool CheckStreamData(StreamData record)
        {
            VirtualContainer vc = new VirtualContainer(record.VcLevel);
            return OutputCredentials[record.Port].SetVirtualContainer(record.VcLevel, record.HigherPath, record.LowerPath, vc);
            
        }

        private ExecutionResult ClearCredentials(StreamData record)
        {
            List<int> avaliblePorts = new List<int>();
            
            bool outputOccp = ((Frame)OutputCredentials[record.Port]).IsFrameOccupied(NetworkDefaultLevel);
            Frame outputCredential = (Frame)OutputCredentials[record.Port];
            
            bool result = outputCredential.ClearVirtualContainer(record.VcLevel, 
                record.HigherPath, 
                record.LowerPath);

            if (outputOccp && !((Frame)OutputCredentials[record.Port]).IsFrameOccupied(NetworkDefaultLevel))
            {
                avaliblePorts.Add(record.Port);
            }
            return new ExecutionResult {
                Ports = avaliblePorts,
                Result = true
            };
        }


        public Dictionary<int, StmLevel> GetPorts()
        {
            return Ttf.GetPorts();
        }

        public List<StreamData> GetStreamData()
        {
            return Streams;
        }

        public ExecutionResult RemoveStreamData(List<StreamData> streams)
        {
            List<int> avaliblePorts = new List<int>();
            foreach (StreamData stream in streams)
            {
                if (Streams.Contains(stream))
                {
                    Streams.Remove(stream);
                    avaliblePorts.AddRange(ClearCredentials(stream).Ports);
                }
            }

            return new ExecutionResult
            {
                Result = true,
                Ports = avaliblePorts
            };
        }

        private void HandleLrmData(object sender, InputLrmArgs args)
        {
            string message = BuildLrmMessage(args.PortNumber);
            Ttf.SendLrmData(args.PortNumber , message);
        }

        private string BuildLrmMessage(int portNumber)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("CLIENT");
            builder.Append("%");
            builder.Append(routerId);
            builder.Append("%");
            builder.Append(portNumber);
            return builder.ToString();
        }

        public void ConnectClient()
        {
            LrmClient.Start();
        }

        private void ReportLrmToken(object sender, InputLrmArgs args)
        {
            LrmToken token = JsonConvert.DeserializeObject<LrmToken>(args.Data);
            token.Reciver = new LrmDestination();
            token.Reciver.Name = routerId;
            token.Reciver.Port = args.PortNumber.ToString();
            LrmClient.SendLrmMessage(token);
        }

        public void IntroduceToLrm()
        {
            LrmClient.SendLrmMessage(LrmIntroduce);
        }
    }
}
