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
        public DropConnectionWindow(SocketHandler handler, string connections, string name, MainWindow main, string type)
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
            if (!String.IsNullOrEmpty(con_string))
            {

                connectionsWithSharp = con_string.Split('|');

                connections = new string[con_string.Equals("") ? 0 : connectionsWithSharp.Length];

                if (elementType.Equals("router"))
                {
                    for (int i = 0; i < connectionsWithSharp.Length; i++)
                    {
                        int position = i + 1;
                        string[] tmp = connectionsWithSharp[i].Split('#');
                        connections[i] = "Polaczanie " + position + ". kontener: " + tmp[2] + "\n" +
                        "Numer 1. portu: " + tmp[0] + ", HP: " + tmp[4] + ", LP: " + tmp[3] +
                        "\nNumer 2.  portu: " + tmp[1] + ", HP: " + tmp[6] + ", LP: " + tmp[5];
                    }
                }
                else if (elementType.Equals("client"))
                {
                    for (int i = 0; i < connectionsWithSharp.Length; i++)
                    {
                        int position = i + 1;
                        string[] tmp = connectionsWithSharp[i].Split('#');
                        connections[i] = "Polaczanie " + position + ".\n" +
                        "Numer portu: " + tmp[0] + ". STM na porcie: " + tmp[1] + ". Kontener: " + tmp[2] + ".\n" +
                        "HigherPath: " + tmp[3] + ". LowerPath: " + tmp[4];

                    }
                }
                connectionsBox.ItemsSource = connections.ToList();
            }

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectionsBox.SelectedItem == null)
            {
                MessageBox.Show("Musisz zaznaczyć połaczenie które chcesz usunąć!");
                return;
            }

            string selected = connectionsWithSharp[connectionsBox.SelectedIndex];


            if (elementType.Equals("router"))
            {
                sockethandler.sendCommand(nodename, "close-connection|" + selected, true);
            }
            else if (elementType.Equals("client"))
            {
                sockethandler.sendCommand(nodename, "delete-resource|" + selected, true);
            }
            this.Close();

        }

        private void connectionsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
