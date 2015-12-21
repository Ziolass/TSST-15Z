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
        private string elementType;
        private MainWindow mainw;
        public DropConnectionWindow(SocketHandler handler,string connections ,string name, MainWindow main,string type)
        {
            
            InitializeComponent();
            elementType = type;
            sockethandler = handler;
            nodename = name;
            mainw = main;
            stringToConnectionArray(connections);
        }
        private void stringToConnectionArray(String con_string)
        {
           connectionsWithSharp = con_string.Split('|');

            connections = new string[con_string.Equals("") ? 0 : connectionsWithSharp.Length];

            if (elementType.Equals("router"))
            {
                for (int i = 0; i < connectionsWithSharp.Length; i++)
                {
                    int position = i + 1;
                    string[] tmp = connectionsWithSharp[i].Split('#');
                    connections[i] = "Polaczanie " + position + ".\n" +
                        "z: " + tmp[0] + " do " + tmp[1] + "\n" +
                        "z pozycji " + tmp[2] + ". na pozycje " + tmp[3] + ".\n" +
                        "Kontener: " + tmp[4] +
                        "Moduł: " + tmp[5];

                }
            }
            else if (elementType.Equals("client"))
            {
                for (int i = 0; i < connectionsWithSharp.Length; i++)
                {
                    int position = i + 1;
                    string[] tmp = connectionsWithSharp[i].Split('#');
                    connections[i] = "Polaczanie " + position + ".\n" +
                        "identyfikator: " + tmp[0] + "\n"+
                        "z: " + tmp[1] + " do " + tmp[2] + "\n" +
                        "z pozycji " + tmp[3] + ". na pozycje " + tmp[4] + ".\n" +
                        "Kontener: " + tmp[5] +
                        "Moduł: " + tmp[6];

                }
            }
            connectionsBox.ItemsSource = connections.ToList();

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(connectionsBox.SelectedItem== null)
            {
                MessageBox.Show("Musisz zaznaczyć połaczenie które chcesz usunąć!");
                return;
            }

            string selected = connectionsWithSharp[connectionsBox.SelectedIndex];

            string []con  =selected.Split('#');
                    
            if (elementType.Equals("router"))
            {
                sockethandler.sendCommand(nodename, "close-connection|" + selected, true);
            }
            else if (elementType.Equals("client"))
            {
                sockethandler.sendCommand(nodename, "delete-connection|" + con[0], true);
            }
            this.Close();

        }

        private void connectionsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
