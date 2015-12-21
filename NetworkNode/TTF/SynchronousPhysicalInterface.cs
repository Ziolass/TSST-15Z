﻿
using NetworkNode.SDHFrame;
using NetworkNode.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
    public class InputDataArgs : EventArgs
    {
        public int PortNumber { get; set; }
        public InputDataArgs(int portNumber)
        {
            PortNumber = portNumber;
        }
    }

    public delegate void HandleInputData(object sender, InputDataArgs args);

    public class SynchronousPhysicalInterface
    {
        private Dictionary<int, List<string>> buffers;
        private Dictionary<int, Output> outputs;
        private Dictionary<int, Input> inputs;
        public String ReceivedFrame { get; private set; }
        public event HandleInputData HandleInputData;

        public SynchronousPhysicalInterface(List<Input> inputs, Dictionary<int, Output> outputs)
        {

            this.outputs = outputs;
            buffers = new Dictionary<int, List<string>>();
            this.inputs = new Dictionary<int, Input>();
            foreach (Input input in inputs)
            {
                buffers.Add(input.InputPort, new List<string>());
                this.inputs.Add(input.InputPort, input);
                input.HandleIncomingData += new HandleIncomingData(pullData);
            }
        }



        private void pullData(object sender, EventArgs args)
        {
            Input source = (Input)sender;
            StringBuilder translator = new StringBuilder();
            byte[] data = source.GetDataFromBuffer();
            translator.Append(Encoding.ASCII.GetString(data, 0, data.Length));
            buffers[source.InputPort].Add(translator.ToString());
            if (HandleInputData != null)
            {
                HandleInputData(this, new InputDataArgs(source.InputPort));
            }

        }

        public void SendFrame(String sdhFrame, int port)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(sdhFrame);
            outputs[port].sendData(byteData);
        }
        //TODO: zweryfikować czy faktycznie usuniemy poprawnie dane z buffora
        public string GetBufferedData(int inputPort)
        {
            string result;
            List<string> bufferedData = buffers[inputPort];

            if (bufferedData.Count > 0)
            {
                result = bufferedData[0];
                bufferedData.RemoveAt(0);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public bool ShudownInterface(int number)
        {
            try
            {
                if (outputs.ContainsKey(number))
                {
                    outputs[number].Active = false;
                    return true;
                }

                if (inputs.ContainsKey(number))
                {
                    inputs[number].Active = false;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<List<int>> GetPorts()
        {
            List<List<int>> results = new List<List<int>>();
            results.Add(new List<int>(inputs.Keys));
            results.Add(new List<int>(outputs.Keys));
            return results;
        }
    }
}
