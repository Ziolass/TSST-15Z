using Microsoft.Win32;
using SDHManagement2.SocketUtils;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ResourceRelocationWindow.xaml
    /// </summary>
    public partial class ResourceRelocationWindow : Window
    {
        private SocketHandler handler;
        private string[] connections;
        private string[] ports;
        private int[] STMs;
        private int[] STMszczelinyBegin;
        private string[] portsWithSTM;

        private string[] conteners ;
        private string[] modules ;
        private string nodeName;
        private int modulesMultiplier ;

        private int[] vc32levels = { 0, 1, 2 };
        private int[] vc4levels = { 0 };
        private int[] vc21levels;
        private int[] vc12levels;

        public ResourceRelocationWindow(SocketHandler handler_, string name,string port_response,string con_respone,List<string> modulesList, List<string> contenersList)
        {
            InitializeComponent();
            initAll(name, port_response,con_respone, modulesList, contenersList);
            handler = handler_;


        }
        private void initAll(string name, string port_response,string con_response, List<string> modulesList, List<string> contenersList)
        {
            modules = modulesList.ToArray();
            conteners = contenersList.ToArray();
            initModuleMultiplier(modulesList);
            contenerTypeBox.ItemsSource = conteners.ToList();
            stringToPortArray(port_response);
            nodeName = name;
            stringToConnectionArray(con_response);
            nodeNameLabel.Content = "Nazwa: "+name;
            initArrays();
        }

        private void stringToConnectionArray(String con_string)
        {

            string[] temp_connections = con_string.Split('|');
            connections = new string[con_string.Equals("") ? 0 : temp_connections.Length];

            for (int i = 0; i < connections.Length; i++)
            {

                string[] tmp = temp_connections[i].Split('#');
                int temp = i + 1;
                connections[i] = "Polaczanie " + temp + ".\n" +
                    "Numer portu: " + tmp[0] + ". STM na porcie: " + tmp[1] + ". Kontener: " + tmp[2] + ".\n" +
                    "HigherPath: " + tmp[3] + ". LowerPath: " + tmp[4];

            }
            connectionsBox.ItemsSource = connections.ToList();

        }
        private void initModuleMultiplier(List<string> modules)
        {
            string stm = modules[0].Substring(3);
            modulesMultiplier = int.Parse(stm);
        }
        private void stringToPortArray(String port_string)
        {

            portsWithSTM = port_string.Split('|');

            ports = new string[portsWithSTM.Length];
            STMs = new int[portsWithSTM.Length];

            for (int i = 0; i < portsWithSTM.Length; i++)
            {
                string[] tmp = portsWithSTM[i].Split('#');
                if (tmp[0] == "")
                {
                    continue;
                }
                ports[i] = tmp[0];
                STMs[i] = int.Parse(tmp[1].Substring(3));
                portsWithSTM[i] = tmp[0] + " " + tmp[1];
            }

            inportBox.ItemsSource = portsWithSTM.ToList();
        }
        
        private void button_Click(object sender, RoutedEventArgs e)
        {
            int inIndex;
            int outIndex;
            int fromlevel;
            int tolevel;

            try {
                if ((int.TryParse(inportBox.SelectedIndex.ToString(), out inIndex)) && int.TryParse(startLevelBox.SelectedItem.ToString(), out fromlevel) && int.TryParse(endLevelBox.SelectedItem.ToString(), out tolevel))
                {
                    //sub-connection-HPC|{port_z1}#{port_do1}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}
                    string tmp = portsWithSTM[inIndex];
                    string stm = tmp.Split(' ')[1];
                    string command = "resource-relocation|" + ports[inIndex] + "#" + stm + "#" +contenerTypeBox.SelectedItem.ToString() +"#" + tolevel + "#" + fromlevel;

                    handler.sendCommand(nodeName, command, true);

                    this.Close();
                }

                else
                {
                    MessageBox.Show("Niepoprawny format!");
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Niepoprawny format!");
                return;
            }
        }
        private void initArrays()
        {
            vc12levels = new int[63];
            vc21levels = new int [21];

            for (int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }
            for (int j = 0; j < vc21levels.Length; j++)
            {
                vc21levels[j] = j;
            }
        }
        private void reInitvc12()
        {
            
            vc12levels = new int[63 * modulesMultiplier];

            for (int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }

        }
        private void reInitvc21()
        {
            vc21levels = new int[21 * modulesMultiplier];

            for (int j = 0; j < vc21levels.Length; j++)
            {
                vc21levels[j] = j;
            }
        }
        private void reInitvc32()
        {
            vc32levels = new int[3 * modulesMultiplier];


            for (int j = 0; j < vc32levels.Length; j++)
            {
                vc32levels[j] = j;
            }
        }
        private void reInitvc4()
        {
            vc4levels = new int[1 * modulesMultiplier];


            for (int j = 0; j < vc4levels.Length; j++)
            {
                vc4levels[j] = j;
            }
        }

        private void contenerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (contenerTypeBox.SelectedItem.ToString())
            {
                case "VC4":
                    reInitvc4();
                   // endLevelBox.ItemsSource = vc4levels.ToList();
                    startLevelBox.ItemsSource = vc4levels.ToList();
                    break;
                case "VC32":
                    reInitvc32();
                    //endLevelBox.ItemsSource = vc32levels.ToList();
                    startLevelBox.ItemsSource = vc32levels.ToList();
                    break;

                case "VC21":
                    reInitvc21();
                    //endLevelBox.ItemsSource = vc21levels.ToList();
                    startLevelBox.ItemsSource = vc21levels.ToList();
                    break;
                case "VC12":
                    reInitvc12();
                    //endLevelBox.ItemsSource = vc12levels.ToList();
                    startLevelBox.ItemsSource = vc12levels.ToList();
                    break;
                default:

                    break;
            }
        }

        private void predButton_Click(object sender, RoutedEventArgs e)
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
                handler.sendCommand(nodeName, result, true);
                this.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void inportBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = inportBox.SelectedIndex;
            int multiplier = STMs[selection];
            STMszczelinyBegin = new int[multiplier];
            for (int i = 0; i < STMszczelinyBegin.Length; i++)
            {
                STMszczelinyBegin[i] = i;
            }
            endLevelBox.ItemsSource = STMszczelinyBegin.ToList();

        }
    }
}
