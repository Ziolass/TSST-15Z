using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NetworkClientNode.Adaptation;
using NetworkClientNode.ViewModelUtils;

namespace NetworkClientNode.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private ClientSetUpProcess ClientSetUpProccess;

        public ObservableCollection<StreamDataViewModel> Streams { get; set; }
        public string MessageSendText { get; set; }
        public string MessageRecivedText { get; set; }
        public ExternalCommand SendMessage { get; set; }

        private StreamDataViewModel selectedStream;
        public StreamDataViewModel SelectedStream
        {
            get { return selectedStream; }
            set
            {
                selectedStream = value;
                risePropertyChange(this, "SelectedStream");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientViewModel"/> class.
        /// </summary>
        public ClientViewModel()
        {
            var args = Environment.GetCommandLineArgs();
            this.Streams = new ObservableCollection<StreamDataViewModel>();
            this.ClientSetUpProccess = new ClientSetUpProcess("..\\..\\..\\Configs\\Client\\clientConfig" + /*args[1]*/0 + ".xml");
            this.ClientSetUpProccess.StreamsCreated += new StreamsCreatedHandler(OnStreamsCreated);
            this.ClientSetUpProccess.StreamCreated += new StreamCreatedHandler(OnStreamCreated);
            this.ClientSetUpProccess.StartClientProcess();
            this.SendMessage = new ExternalCommand(SendNewMessage, IsMessageReady);
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
        /// <summary>
        /// Determines whether is message ready. TODO sprawdza czy tak jest!!!
        /// </summary>
        /// <returns></returns>
        private bool IsMessageReady()
        {
            return true;
        }
        private void SendNewMessage()
        {
            this.ClientSetUpProccess.ClientNode.SelectStream(this.selectedStream.StreamData);
            this.ClientSetUpProccess.ClientNode.SendData(this.MessageSendText);
        }
        private bool IsSelectStreamRady()
        {
            return true;
        }
        private void SelectStream()
        {

        }
        private void risePropertyChange(object sender, String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(property));
            }
        }


    }
}
