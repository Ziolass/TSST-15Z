using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkClientNode.Adaptation;

namespace NetworkClientNode.ViewModels
{
    public class StreamDataViewModel : INotifyPropertyChanged
    {
        public StreamData StreamData { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public StreamDataViewModel(StreamData streamData)
        {
            this.StreamData = streamData;
        }

        public String Id
        {
            get
            {
                return StreamData.Port + " " + StreamData.VcLevel + " " + StreamData.Stm + " [" + StreamData.HigherPath + "," + StreamData.LowerPath + "]";
            }
        }
        public void riseChangesToView()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
        }
    }
}
