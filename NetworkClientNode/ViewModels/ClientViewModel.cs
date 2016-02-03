using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NetworkClientNode.Adaptation;
using NetworkClientNode.ViewModelUtils;
using System.Windows.Input;

namespace NetworkClientNode.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private ClientSetUpProcess ClientSetUpProccess;

        public ObservableCollection<StreamDataViewModel> Streams { get; set; }
        public string MessageSendText { get; set; }
        public ExternalCommand SendMessage { get; set; }
        private StreamDataViewModel selectedStream;
        public StreamDataViewModel SelectedStream
        {
            get { return selectedStream; }
            set
            {
                selectedStream = value;
                RisePropertyChange(this, "SelectedStream");
            }
        }
        private string messageRecivedText;
        public string MessageRecivedText
        {
            get { return messageRecivedText; }
            set { messageRecivedText = value; }
        }

        private string messageConsoleText;
        public string MessageConsoleText
        {
            get { return messageConsoleText; }
            set { messageConsoleText = value; }
        }
        
        public string ClientName
        {
            get { return this.ClientSetUpProccess.ClientNode.Id; }
        }
        


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientViewModel" /> class.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Wrong application start argument
        /// </exception>
        public ClientViewModel()
        {
            try
            {
                //var args = Environment.GetCommandLineArgs();
                string[] args = { "0", "0", "0" };
                int i = 0; //This is dumy variable for TryParse
                if (args.Length < 2)
                    throw new Exception("Wrong application start argument");
                else if (!int.TryParse(args[1], out i))
                    throw new Exception("Wrong application start argument");

                this.Streams = new ObservableCollection<StreamDataViewModel>();
                this.ClientSetUpProccess = new ClientSetUpProcess("..\\..\\..\\Configs\\NetworkClient\\clientConfig" + args[1] + ".xml");
                this.ClientSetUpProccess.StreamsCreated += new StreamsCreatedHandler(OnStreamsCreated);
                this.ClientSetUpProccess.StreamCreated += new StreamCreatedHandler(OnStreamCreated);
                this.ClientSetUpProccess.StartClientProcess();
                this.ClientSetUpProccess.ClientNode.StreamAdded += new StreamChangedHandler(OnStreamAdded);
                this.ClientSetUpProccess.ClientNode.StreamRemoved += new StreamChangedHandler(OnStreamRemoved);
                this.ClientSetUpProccess.ClientNode.RegisterDataListener(new HandleClientData(OnHandleClientData));
                this.SendMessage = new ExternalCommand(SendNewMessage, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OnStreamRemoved(StreamChangedArgs args)
        {
            foreach (StreamData stream in args.Streams)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.Streams.Remove(new StreamDataViewModel(stream));
                });
            }
        }

        /// <summary>
        /// Called when client recive message.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnHandleClientData(ClientData data)
        {
            this.messageRecivedText += DateTime.Now + "\n" + data.ToString();
            RisePropertyChange(this, "MessageRecivedText");
        }



        private void OnStreamAdded(StreamChangedArgs args)
        {
            foreach (StreamData stream in args.Streams)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.Streams.Add(new StreamDataViewModel(stream));
                });
            }
        }

        /// <summary>
        /// Called when streams created.
        /// </summary>
        private void OnStreamsCreated()
        {
            foreach (StreamData streamData in this.ClientSetUpProccess.ClientNode.GetStreamData())
            {
                this.Streams.Add(new StreamDataViewModel(streamData));
            }
        }
        /// <summary>
        /// Called when stream created.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private void OnStreamCreated(StreamCreatedArgs args)
        {
            this.Streams.Add(new StreamDataViewModel(args.StreamData));
        }
        private void SendNewMessage()
        {
            this.ClientSetUpProccess.ClientNode.SelectStream(this.selectedStream.StreamData);
            this.ClientSetUpProccess.ClientNode.SendData(this.MessageSendText);
        }
        private void RisePropertyChange(object sender, String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(property));
            }
        }
    }
}
