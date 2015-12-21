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
using SDHManagement2.SocketUtils;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.IO;

namespace SDHManagement2.AdditionalWindows
{
    /// <summary>
    /// Interaction logic for AddConnectionWindow.xaml
    /// </summary>
    public partial class AddConnectionWindow : Window
    {
        private SocketHandler handler;
        private string [] connections;
        private string [] inports;
        private string[] outports;
        private string[] conteners = { "VC4", "VC32", "VC21", "VC12" };
        private string[] modules = {"STM1", "STM4", "STM16","STM64","STM256"};
        private int[] modulesInt = { 1, 4, 16, 64, 256 };
        private int[] vc32levels = { 0,1,2};
        private int[] vc4levels = { 0 };
        private int[] vc21levels;
        private int []vc12levels;

       

        public AddConnectionWindow(SocketHandler handler_, string port_response, string connection_response,string name)
        {
            InitializeComponent();
            contenerTypeBox.ItemsSource = conteners.ToList();
            STMComboBox.ItemsSource = modules.ToList();
            STMComboBox.SelectedIndex =0;
            handler = handler_;
            nodeNameLabel.Content = name;
            stringToPortArray(port_response);
            stringToConnectionArray(connection_response);
            initArrays();


        }

        private void stringToPortArray(String port_string)
        {
            string[] temp_ports = port_string.Split('|');
            inports = temp_ports[0].Split('#');
            outports = temp_ports[1].Split('#');

            inportBox.ItemsSource = inports.ToList();
            outportBox.ItemsSource = outports.ToList();

        }

        private void stringToConnectionArray(String con_string)
        {

            string[] temp_connections = con_string.Split('|');
            connections = new string[con_string.Equals("") ? 0 : temp_connections.Length];

            for (int i = 0; i < connections.Length; i++)
            {

                string[] tmp = temp_connections[i].Split('#');
                int temp = i + 1;
                // connections[i] = "Połączenie "+temp+".\n"+
                // +tmp[0] + " --> " + tmp[1];
                connections[i] = "Połączenie " + temp + ".\n" +
                    "Z portu " + tmp[0] + ". na port " + tmp[1] + ".\n" +
                    "Ze szczeliny " + tmp[2] + ". do szczeliny " + tmp[3] + ".\n" +
                    "Moduł: " + tmp[5] + "\n" +
                    "Kontener: " + tmp[4];


            }
            connectionsBox.ItemsSource = connections.ToList();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int inport; 
            int outport;
            int fromlevel;
            int tolevel;

            try {
                if ((int.TryParse(inportBox.SelectedItem.ToString(), out inport)) && int.TryParse(outportBox.SelectedItem.ToString(), out outport) && int.TryParse(startLevelBox.SelectedItem.ToString(), out fromlevel) && int.TryParse(endLevelBox.SelectedItem.ToString(), out tolevel))
                {
                    //sub-connection-HPC|{port_z1}#{port_do1}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}
                    string command = "sub-connection-HPC|" + inport + "#" + outport + "#" + fromlevel + "#" + tolevel + "#" + contenerTypeBox.SelectedItem.ToString() + "#" + STMComboBox.SelectedItem.ToString() ;
                    handler.sendCommand(nodeNameLabel.Content.ToString(), command, true);

                    this.Close();
                }

                else
                {
                    MessageBox.Show("Dozwolone są tylko wartości numeryczne");
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Dozwolone są tylko wartości numeryczne");
                return;
            }
        }
        private void reInitvc12()
        {

            int SMIdentifier = modulesInt[ STMComboBox.SelectedIndex];

            vc12levels = new int[63 * SMIdentifier];

            for (int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }

        }
        private void reInitvc21()
        {
            int SMIdentifier = modulesInt[STMComboBox.SelectedIndex];
            vc21levels = new int[21 * SMIdentifier];

            for (int j = 0; j < vc21levels.Length; j++)
            {
                vc21levels[j] = j;
            }
        }
        private void reInitvc32()
        {
            int SMIdentifier = modulesInt[STMComboBox.SelectedIndex];
            vc32levels = new int[3 * SMIdentifier];


            for (int j = 0; j < vc32levels.Length; j++)
            {
                vc32levels[j] = j;
            }
        }
        private void reInitvc4()
        {
            int SMIdentifier = modulesInt[STMComboBox.SelectedIndex];
            vc4levels = new int[1 * SMIdentifier];


            for (int j = 0; j < vc4levels.Length; j++)
            {
                vc4levels[j] = j;
            }
        }


        private void initArrays()
        {
            vc12levels = new int[63];
            vc21levels = new int[21];

            for(int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }
            for(int j =0; j< vc21levels.Length; j++)
            {
                vc21levels[j] = j;
            }
        }
        private void contenerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (contenerTypeBox.SelectedItem.ToString())
            {
                case "VC4":
                    reInitvc4();
                    endLevelBox.ItemsSource = vc4levels.ToList();
                    startLevelBox.ItemsSource = vc4levels.ToList();
                    break;
                case "VC32":
                    reInitvc32();
                    endLevelBox.ItemsSource = vc32levels.ToList();
                    startLevelBox.ItemsSource = vc32levels.ToList();
                    break;

                case "VC21":
                    reInitvc21();
                    endLevelBox.ItemsSource = vc21levels.ToList();
                    startLevelBox.ItemsSource = vc21levels.ToList();
                    break;
                case "VC12":
                    reInitvc12();
                    endLevelBox.ItemsSource = vc12levels.ToList();
                    startLevelBox.ItemsSource = vc12levels.ToList();
                    break;
                default:
                  
                    break;
            }
        }

        private void STMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            string path = new DirectoryInfo(((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).Parent).FullName + "\\Configs").ToString();
            openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

                try
                {

                string result = "";
                if (openFileDialog.ShowDialog() == true)
                result = File.ReadAllText(openFileDialog.FileName);
                handler.sendCommand(nodeNameLabel.Content.ToString(), result, true);
                this.Close();
                }
                catch(Exception ex)
                {
                
                }
        }
    }
}
