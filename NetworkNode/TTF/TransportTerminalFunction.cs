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
        MULTIPLEXER
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

    public delegate void HandleInputFrame(object sender, InputFrameArgs args);
    public class TransportTerminalFunction
    {
        private SynchronousPhysicalInterface spi;        
        private RegeneratorSectionTermination rst;
        private MultiplexSectionTermination mst;

        private IFrameBuilder builder;
        private NodeMode nodeMode;

        public event HandleInputFrame HandleInputFrame;
        public TransportTerminalFunction(SynchronousPhysicalInterface spi, NodeMode mode)
        {
            this.spi = spi;
            rst = new RegeneratorSectionTermination();
            this.nodeMode = mode;
            if (nodeMode == NodeMode.MULTIPLEXER)
            {
                mst = new MultiplexSectionTermination();
            }
            this.spi.HandleInputData += new HandleInputData(getInputData);
            builder = new FrameBuilder();
        }

        private void getInputData(object sender, InputDataArgs args)
        {
            string bufferedData = spi.GetBufferedData(args.PortNumber);

            IFrame result = beginFrameEvaluation(bufferedData);

            if (HandleInputFrame != null)
            {
                HandleInputFrame(this, new InputFrameArgs(args.PortNumber, result));
            }
        }

        private IFrame beginFrameEvaluation(string bufferedData)
        {
            IFrame frame = builder.BuildFrame(bufferedData);

            rst.evaluateHeader(frame);
            
            if (nodeMode == NodeMode.MULTIPLEXER)
            {
                mst.evaluateHeader(frame);
            }
            raportFrame(frame, "Input Frame:");

            return frame;
        }

        private void raportFrame(IFrame frame, string raprtHeader)
        {
            Console.WriteLine(raprtHeader);
            Console.WriteLine(frame.ToString());
        }

        public List<List<int>> GetPorts()
        {
            return spi.GetPorts();
        }
        public void PassDataToInterfaces(Dictionary<int,IFrame> outputFrames)
        {
            foreach (int outputPort in outputFrames.Keys)
            {
                IFrame frame = outputFrames[outputPort];
                rst.generateHeader(ref frame);
                if (nodeMode == NodeMode.MULTIPLEXER)
                {
                    mst.generateHeader(ref frame);
                }
                
                String textForm = builder.BuildLiteral(frame);
                spi.SendFrame(textForm, outputPort);
                raportFrame(frame,"Output Frame");
            }
            //TODO tu może być zgłaszanie zdarzeia wysłania i powiadaomienie zegara zewnętrznego że wysyłamy ramkę.
        }
    }
}
