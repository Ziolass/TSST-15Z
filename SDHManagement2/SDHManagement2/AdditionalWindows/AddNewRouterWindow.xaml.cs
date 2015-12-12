using SDHManagement2.SocketUtils;
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

namespace SDHManagement2.AdditionalWindows
{
    /// <summary>
    /// Interaction logic for AddNewRouterWindow.xaml
    /// </summary>
    public partial class AddNewRouterWindow : Window
    {
        private MainWindow mainw;
        private SocketHandler handler;
        public AddNewRouterWindow(MainWindow main, SocketHandler hand)
        {
            InitializeComponent();
            mainw = main;
            handler = hand;

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string name = textBox.Text;
            int port;

            if(!int.TryParse(textBox1.Text,out port))
            {
                MessageBox.Show("Only numeric values allowed, please try again");
                textBox1.Text = "";
                return;
            }

            string result = handler.addSingleNode(name, port);
            mainw.appendConsole(result,null,null);
            this.Close();




            
        }
    }
}
