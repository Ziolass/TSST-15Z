using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkClientNode.Adaptation;

namespace NetworkClientNode.ViewModels
{
    public class StreamDataViewModel : INotifyPropertyChanged, IEquatable<StreamDataViewModel>
    {
        public StreamData StreamData { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string ClientName { get; set; }

        public StreamDataViewModel(string clientName, StreamData streamData)
        {
            this.ClientName = clientName;
            this.StreamData = streamData;
        }

        public String Id
        {
            get
            {
                return ClientName + "(" + StreamData.Port + " " + StreamData.VcLevel + " " + StreamData.Stm + " [" + StreamData.HigherPath + "," + StreamData.LowerPath + "])";
                //return StreamData.Port + " " + StreamData.VcLevel + " " + StreamData.Stm + " [" + StreamData.HigherPath + "," + StreamData.LowerPath + "]";
            }
        }
        public void riseChangesToView()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
        }

        public bool Equals(StreamDataViewModel other)
        {
            if (StreamData.Equals(other.StreamData))
            {
                return true;
            }
            else return false;
        }
    }
}
