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
using NetworkClientNode.CPCC;
using System.Threading;

namespace NetworkClientNode.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private ClientSetUpProcess ClientSetUpProccess;
        private CallingPartyCallController Cpcc;
        public ObservableCollection<StreamDataViewModel> Streams { get; set; }
        public string MessageSendText { get; set; }
        public string ClientToConnect { get; set; }
        public ExternalCommand SendMessage { get; set; }
        public ExternalCommand Connect { get; set; }
        public ExternalCommand Disconnect { get; set; }
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
        public string messageRecivedText;
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

        private string clientName;

        public string ClientName
        {
            get { return clientName; }
            set { clientName = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string NextConnectionClient { get; set; }

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
                var args = Environment.GetCommandLineArgs();
                int i = 0; //This is dumy variable for TryParse
                if (args.Length < 2)
                    throw new Exception("Wrong application start argument");

                this.Cpcc = new CallingPartyCallController(args[2], this);
                this.Streams = new ObservableCollection<StreamDataViewModel>();
                this.ClientSetUpProccess = new ClientSetUpProcess("..\\..\\..\\Configs\\NetworkClient\\clientConfig" + args[1] + ".xml");
                this.ClientName = this.ClientSetUpProccess.ClientName;
                //this.ClientSetUpProccess.StreamsCreated += new StreamsCreatedHandler(OnStreamsCreated);
                //this.ClientSetUpProccess.StreamCreated += new StreamCreatedHandler(OnStreamCreated);
                this.ClientSetUpProccess.StartClientProcess();
                //this.ClientSetUpProccess.ClientNode.StreamAdded += new StreamChangedHandler(OnStreamAdded);
                //this.ClientSetUpProccess.ClientNode.StreamRemoved += new StreamChangedHandler(OnStreamRemove);
                this.ClientSetUpProccess.ClientNode.RegisterDataListener(new HandleClientData(OnHandleClientData));

                this.ClientSetUpProccess.ClientNode.Adaptation.AllocationClientStream += new AllocationClientStream(OnClientStreamAdd);
                this.ClientSetUpProccess.ClientNode.Adaptation.DeallocationClientStream += new DeallocationClientStream(OnClientStreamRemove);

                this.Cpcc.ConnectionEstablished += new ConnectionEstablished(OnConnectionEstablished);
                this.Cpcc.ConnectionRemoved += new ConnectionRemoved(OnConnectionRemoved);

                this.SendMessage = new ExternalCommand(SendNewMessage, true);
                this.Connect = new ExternalCommand(ConnectNew, true);
                this.Disconnect = new ExternalCommand(TearDownConnection, true);
            }
            catch (Exception e)
            {
                this.messageConsoleText += DateTime.Now + e.Message + "\n";
                RisePropertyChange(this, "MessageConsoleText");
            }
        }

        private void OnConnectionRemoved(string connectionName)
        {

        }

        private void OnConnectionEstablished(string connectionName)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                /*if (this.Streams.Count > 0)
                {
                    this.Streams[this.Streams.Count - 1].ClientName = connectionName;
                    this.Streams[this.Streams.Count - 1].riseChangesToView();
                    //RisePropertyChange(this, "Streams");
                }
                else*/ this.NextConnectionClient = connectionName;
            });
        }
        private void OnClientStreamRemove(List<StreamData> args)
        {
            foreach (StreamData stream in args)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.Streams.Remove(new StreamDataViewModel(this.NextConnectionClient, stream));
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
            Console.WriteLine(data.ToString());
            string newString = data.ToString().Replace("Error: Container does not transport client data", "");
            if (!string.IsNullOrEmpty(newString) && newString != "\n")
            {
                newString = newString.ToString().Replace("\n", " ");
                this.messageRecivedText += DateTime.Now + "\n" + newString.ToString() + "\n";
                RisePropertyChange(this, "MessageRecivedText");
            }
        }
        private void OnClientStreamAdd(List<StreamData> args)
        {
            foreach (StreamData stream in args)
            {
                this.messageConsoleText += DateTime.Now + ": Nowe po³¹czenie\n";
                RisePropertyChange(this, "MessageConsoleText");
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.Streams.Add(new StreamDataViewModel(this.NextConnectionClient, stream));                    
                });
            }

        }
        private void TearDownConnection()
        {
            if (this.selectedStream.ClientName != null)
            {

                string result = Cpcc.callTeardown(this.selectedStream.ClientName);
                this.messageConsoleText += DateTime.Now + ": " + result + "\n";
                RisePropertyChange(this, "MessageRecivedText");
                RisePropertyChange(this, "Streams");
            }
        }

        private void SendNewMessage()
        {
            this.ClientSetUpProccess.ClientNode.SelectStream(this.selectedStream.StreamData);
            this.ClientSetUpProccess.ClientNode.SendData(this.MessageSendText);
        }
        private void ConnectNew()
        {
            if (this.ClientToConnect != null)
            {
                var test = Cpcc.callRequest(this.ClientToConnect);
                this.messageConsoleText += DateTime.Now + ": " + test + "\n";
                RisePropertyChange(this, "MessageRecivedText");
            }
        }
        public void RisePropertyChange(object sender, String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(property));
            }
        }
    }
}
