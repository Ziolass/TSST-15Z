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
    /// Interaction logic for DropConnectionWindow.xaml
    /// </summary>
    public partial class DropConnectionWindow : Window
    {
        private SocketHandler sockethandler;
        private string[] connections;
        private string[] connectionsWithSharp;
        private string nodename;
        private MainWindow mainw;
        public DropConnectionWindow(SocketHandler handler,string connections ,string name, MainWindow main)
        {
            InitializeComponent();
            sockethandler = handler;
            nodename = name;
            mainw = main;
            stringToConnectionArray(connections);
        }
        private void stringToConnectionArray(String con_string)
        {
           connectionsWithSharp = con_string.Split('|');
            connections = new string[connectionsWithSharp.Length];

            for (int i = 0; i < connectionsWithSharp.Length; i++)
            {
                string[] tmp = connectionsWithSharp[i].Split('#');
                connections[i] = tmp[0] + " --> " + tmp[1];
            }
            connectionsBox.ItemsSource = connections.ToList();

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(connectionsBox.SelectedItem== null)
            {
                MessageBox.Show("You have to select one connections to delete it!");
                return;
            }

            string selected = connectionsWithSharp[connectionsBox.SelectedIndex];
            sockethandler.sendCommand(nodename, "shutdown-interface|" + selected,true);
            this.Close();

        }
    }
}
