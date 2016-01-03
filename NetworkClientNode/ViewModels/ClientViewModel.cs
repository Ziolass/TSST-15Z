using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClientNode.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {

        private NetworkClNode ClientNode;
        public string Text { get; set; }
        public ClientViewModel()
        {
            ElementConfigurator creator = new ElementConfigurator("C:\\projekty\\uczelnia\\TSST\\Repozytorium\\Configs\\NetworkClient\\clientConfig0.xml");
            ClientNode = creator.ConfigureNode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
