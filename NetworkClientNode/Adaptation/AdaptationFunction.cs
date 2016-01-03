using NetworkNode.SDHFrame;
using NetworkNode.TTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode.Adaptation
{
    public class AdaptationFunction
    {
        private TransportTerminalFunction Ttf;
        private List<StreamData> Streams;
        private Dictionary<int, IFrame> outputCredentials;
        private FrameBuilder Builder;
        public AdaptationFunction(TransportTerminalFunction ttf)
        {
            Ttf = ttf;
            Builder = new FrameBuilder();
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
                outputData[stream.Port].SetVirtualContainer(stream.VcLevel, stream.HigherPathOut, stream.LowerPathOut, vc);
            }
            
            Ttf.PassDataToInterfaces(outputData);        
        }

        public ExecutionResult AddStreamData(List<StreamData> records)
        {
            int index = 0;
            foreach (StreamData record in records)
            {

                if (!CheckStreamData(record))
                {
                    return new ExecutionResult(false, "Error at record " + index);
                }
                index++;
            }

            Streams.AddRange(records);

            return new ExecutionResult(true, null);
        }

        private bool CheckStreamData(StreamData record)
        {
            VirtualContainer vc = new VirtualContainer(record.VcLevel);
            return outputCredentials[record.Port].SetVirtualContainer(record.VcLevel, record.HigherPathOut, record.LowerPathOut, vc);
            
        }

        private bool ClearCredentials(StreamData record)
        {
            Frame outputCredential = (Frame)outputCredentials[record.Port];
            return outputCredential.ClearVirtualContainer(record.VcLevel, record.HigherPathOut, record.LowerPathOut);
        }


        public Dictionary<int, StmLevel> GetPorts()
        {
            return Ttf.GetPorts();
        }

        public List<StreamData> GetStreamData()
        {
            return Streams;
        }

        public bool RemoveStreamData(StreamData record)
        {
            if (Streams.Contains(record))
            {
                Streams.Remove(record);
                ClearCredentials(record);
                return true;
            }
            return false;
        }
    }
}
