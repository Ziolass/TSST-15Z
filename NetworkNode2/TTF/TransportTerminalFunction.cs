using NetworkNode.SDHFrame;
using NetworkNode.HPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
    public enum NodeMode
    {
        REGENERATOR,
        MULTIPLEXER,
        CLIENT
    }

    public enum DataType
    {
        LRM,
        FRAME
        
    }
    public class InputFrameArgs : InputDataArgs
    {
        public IFrame Frame { get; set; }
        public InputFrameArgs(int inputPort, IFrame frame)
            : base(inputPort)
        {
            Frame = frame;
        }
    }

    public class InputLrmArgs : InputDataArgs
    {
        public string Data { get; set; }
        public InputLrmArgs(int inputPort, string data)
            : base(inputPort)
        {
            Data = data;
        }
    }

    public delegate void HandleInputFrame(object sender, InputFrameArgs args);
    public delegate void HandleLrmData(object sender, InputLrmArgs args);

    public class TransportTerminalFunction
    {
        private SynchronousPhysicalInterface spi;        
        private RegeneratorSectionTermination rst;
        private MultiplexSectionTermination mst;

        private IFrameBuilder builder;
        private NodeMode nodeMode;

        public event HandleInputFrame HandleInputFrame;
        public event HandleLrmData HandleLrmData;
        public TransportTerminalFunction(SynchronousPhysicalInterface spi, NodeMode mode)
        {
            this.spi = spi;
            rst = new RegeneratorSectionTermination();
            this.nodeMode = mode;
            if (nodeMode == NodeMode.MULTIPLEXER)
            {
                mst = new MultiplexSectionTermination();
            }
            this.spi.HandleInputData += new HandleInputData(GetInputData);
            builder = new FrameBuilder();
        }

        public Dictionary<int, StmLevel> GetPorts()
        {
            return spi.GetPorts();
        }
        public void PassDataToInterfaces(Dictionary<int, IFrame> outputFrames)
        {
            foreach (int outputPort in outputFrames.Keys)
            {
                IFrame frame = outputFrames[outputPort];
                rst.generateHeader(frame);
                if (nodeMode == NodeMode.MULTIPLEXER)
                {
                    mst.generateHeader(frame);
                }
                String textForm = builder.BuildLiteral(frame);
                spi.SendFrame(textForm, outputPort);
                raportFrame(frame, "Output Frame");
            }
            //TODO tu może być zgłaszanie zdarzeia wysłania i powiadaomienie zegara zewnętrznego że wysyłamy ramkę.
        }

        public bool ShudownInterface(int number)
        {
            return spi.ShudownInterface(number);
        }

        public void SendLrmData(int portNumber, string lrmData)
        {
            spi.SendLrmData(portNumber, lrmData);
        }

        private void GetInputData(object sender, InputDataArgs args)
        {
            string bufferedData = spi.GetBufferedData(args.PortNumber);

            DataType type = DetectDataType(bufferedData);

            switch (type)
            {
                case DataType.FRAME:
                    {
                        HandleFrameData(bufferedData, args.PortNumber);
                        break;
                    }
                case DataType.LRM:
                    {
                        HandleLrmFrame(bufferedData, args.PortNumber);
                        break;
                    }
            }
        }

        private void HandleFrameData(string bufferedData, int portNumber)
        {
            IFrame result = beginFrameEvaluation(bufferedData);

            if (HandleInputFrame != null)
            {
                HandleInputFrame(this, new InputFrameArgs(portNumber, result));
            }
        }

        private void HandleLrmFrame(string bufferedData, int portNumber)
        {
            string[] normalizedData = bufferedData.Split('@');

            string data = normalizedData[1];
            if (HandleLrmData != null)
            {
                HandleLrmData(this, new InputLrmArgs(portNumber, data));
            }
        }

        private DataType DetectDataType(string data)
        {
            string[] normalizedData = data.Split('@');
            return normalizedData[0].Equals("LRM") ? DataType.LRM : DataType.FRAME;
        }

        private IFrame beginFrameEvaluation(string bufferedData)
        {
            IFrame frame = builder.BuildFrame(bufferedData);
            
            raportFrame(frame, "Input Frame:");

            rst.evaluateHeader(frame);
            
            if (nodeMode == NodeMode.MULTIPLEXER)
            {
                mst.evaluateHeader(frame);
            }
            

            return frame;
        }

        private void raportFrame(IFrame frame, string raportHeader)
        {
            Console.WriteLine(raportHeader);
            Console.WriteLine(((Frame) frame).ToString());
            if(((Frame)frame).Rsoh != null) 
            {
                Console.WriteLine("RSOH = " + ((Frame)frame).Rsoh.ToString());
            }

            if (((Frame)frame).Msoh != null)
            {
                Console.WriteLine("MSOH = " + ((Frame)frame).Msoh.ToString());
            }
        }

        

        internal void AddRsohContent(string dccContent)
        {
            rst.SetNextData(dccContent);
        }

        internal void AddMsohContent(string dccContent)
        {
            mst.SetNextData(dccContent);
        }
    }
}
