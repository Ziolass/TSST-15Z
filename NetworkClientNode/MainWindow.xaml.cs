using NetworkClientNode.CPCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace NetworkClientNode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string WindowName { get; set; }
        public MainWindow()
        {
            //Setup WindowName
            var args = Environment.GetCommandLineArgs();
            int i = 0; //This is dumy variable for TryParse
            if (args.Length < 2)
                throw new Exception("Wrong application start argument");
            ClientSetUpProcess ClientSetUpProccess = new ClientSetUpProcess("..\\..\\..\\Configs\\NetworkClient\\clientConfig" + args[1] + ".xml");
            InitializeComponent();
            this.Title = ClientSetUpProccess.GetNodeName();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
