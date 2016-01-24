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
        public string MyTitle { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            var args = Environment.GetCommandLineArgs();
            int i = 0; //This is dumy variable for TryParse
            if (args.Length < 3)
                throw new Exception("Wrong application start argument");
            else if (!int.TryParse(args[1], out i))
                throw new Exception("Wrong application start argument");

            DataContext = this;
            MyTitle = "Client" + args[2];
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
