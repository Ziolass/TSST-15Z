using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SDHManagement2.SocketUtils;
using Microsoft.Win32;
using System.IO;

namespace SDHManagement2.AdditionalWindows
{
    /// <summary>
    /// Interaction logic for AddConnectionWindow.xaml
    /// </summary>
    public partial class AddConnectionWindow : Window
    {
        #region variables
        private SocketHandler handler;
        private string [] connections;
        private string [] inports;
        private string[] outports;
        private string[] ports;
        private string[] portsWithSTM;
        private int[] STMs;
        private int[] STMszczelinyBegin;
        private int[] STMszczelinyEnd;
        private string nodeName;
        private string[] conteners;
        private string[] modules;
        private int modulesMultiplier;
        private int[] vc32levels = { 0,1,2};
        private int[] vc4levels = { 0 };
        private int[] vc21levels;
        private int []vc12levels;
        #endregion
        public AddConnectionWindow(SocketHandler handler_, string port_response, string connection_response,string name,List<string> modulesList, List<string> contenersList)
        {
            InitializeComponent();
            handler = handler_;
            initAll(port_response, connection_response, name, modulesList, contenersList);
        }
        private void initAll(string port_response, string connection_response, string name, List<string> modulesList, List<string> contenersList)
        {
            STMszczelinyBegin = new int[1] { 0 };
            STMszczelinyEnd = new int[1] { 0 };

            beginHigherPath.ItemsSource = STMszczelinyBegin.ToList();
            endHigherPath.ItemsSource = STMszczelinyEnd.ToList();

            conteners = contenersList.ToArray();
            modules = modulesList.ToArray();
            contenerTypeBox.ItemsSource = conteners.ToList();
           // STMComboBox.ItemsSource = modules.ToList();
            //STMComboBox.SelectedIndex = 0;
            nodeName = name;
            nodeNameLabel.Content = "Nazwa: "+name;
            stringToPortArray(port_response);
            initModuleMultiplier(modulesList);
            stringToConnectionArray(connection_response);
            initArrays();
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

            for(int i = 0; i < portsWithSTM.Length; i++)
            {
                if (portsWithSTM[i].Equals(""))
                {
                    continue;
                }
                string[] tmp = portsWithSTM[i].Split('#');
                ports[i] = tmp[0];
                STMs[i] = int.Parse(tmp[1].Substring(3));
                portsWithSTM[i] = tmp[0] + " - " + tmp[1];
            }

            outportBox.ItemsSource = portsWithSTM.ToList();
            inportBox.ItemsSource = portsWithSTM.ToList();

        }
        private void stringToConnectionArray(String con_string)
        {

            string[] temp_connections = con_string.Split('|');
            connections = new string[con_string.Equals("") ? 0 : temp_connections.Length];

            for (int i = 0; i < connections.Length; i++)
            {

                string[] tmp = temp_connections[i].Split('#');
                int temp = i + 1;
                connections[i] = "Polaczanie " + temp + ". kontener: " + tmp[2] + "\n" +
                  "Numer 1. portu: " + tmp[0] + ", HP: " + tmp[4] + ", LP: " + tmp[3] +
                  "\nNumer 2. portu: " + tmp[1] + ", HP: " + tmp[6] + ", LP: " + tmp[5];

            }
            connectionsBox.ItemsSource = connections.ToList();

        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            int beginHP; 
            string beginLP;
            int endHP;
            string endLP;
            //int inport;
            int inIndex;
            //int outport;
            int outIndex;

            try {

                int.TryParse(inportBox.SelectedIndex.ToString(), out inIndex);
                int.TryParse(outportBox.SelectedIndex.ToString(), out outIndex);
                if(ports[outIndex] == ports[inIndex])
                {
                    MessageBox.Show("Nie mo¿na wybraæ dwóch tych samych portów!");
                    return;
                }

                
                if ((int.TryParse(beginHigherPath.SelectedItem.ToString(), out beginHP)) && int.TryParse(endHigherPath.SelectedItem.ToString(), out endHP) )
                {

                    beginLP = beginLowerPath.SelectedItem==null ? "" : beginLowerPath.SelectedItem.ToString();
                    endLP = endLowerPath.SelectedItem == null ? "" : endLowerPath.SelectedItem.ToString();
                    
                    //sub-connection-HPC|{port_z1}#{port_do1}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}
                    string command = "sub-connection-HPC|" + ports[inIndex] + "#" + ports[outIndex] + "#" + contenerTypeBox.SelectedItem.ToString() + "#" + beginLP + "#" + beginHP + "#" + endLP +"#"+endHP ;
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

                MessageBox.Show("Niepoprawny format! B³¹d: "+ex.Message );
                return;
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
        private void reinitComboBoxes(List<int> s)
        {
           // beginHigherPath.ItemsSource = s;
            beginLowerPath.ItemsSource = s;
            //endHigherPath.ItemsSource = s;
            endLowerPath.ItemsSource = s;
        }
        private void contenerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (contenerTypeBox.SelectedItem.ToString())
            {
                case "VC4":
                    reInitvc4();
                    reinitComboBoxes(vc4levels.ToList());
                   // endLevelBox.ItemsSource = vc4levels.ToList();
                    //startLevelBox.ItemsSource = vc4levels.ToList();
                    break;
                case "VC32":
                    reInitvc32();
                    reinitComboBoxes(vc32levels.ToList());
                    //endLevelBox.ItemsSource = vc32levels.ToList();
                    //startLevelBox.ItemsSource = vc32levels.ToList();
                    break;

                case "VC21":
                    reInitvc21();
                    reinitComboBoxes(vc21levels.ToList());
                   // endLevelBox.ItemsSource = vc21levels.ToList();
                    //startLevelBox.ItemsSource = vc21levels.ToList();
                    break;
                case "VC12":
                    reInitvc12();
                    reinitComboBoxes(vc12levels.ToList());
                    //endLevelBox.ItemsSource = vc12levels.ToList();
                    //startLevelBox.ItemsSource = vc12levels.ToList();
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
                handler.sendCommand(nodeName, result, true);
                this.Close();
                }
                catch(Exception ex)
                {
                
                }
        }

        private void inportBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection= inportBox.SelectedIndex;
            int multiplier = STMs[selection];
            STMszczelinyBegin = new int[multiplier];
            for(int i = 0; i < STMszczelinyBegin.Length; i++)
            {
                STMszczelinyBegin[i] = i;
            }
            beginHigherPath.ItemsSource = STMszczelinyBegin.ToList();

        }

        private void outportBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selection = outportBox.SelectedIndex;
            int multiplier = STMs[selection];
            STMszczelinyEnd = new int[multiplier];
            for (int i = 0; i < STMszczelinyEnd.Length; i++)
            {
                STMszczelinyEnd[i] = i;
            }
            endHigherPath.ItemsSource = STMszczelinyEnd.ToList();
        }

        private void beginHigherPath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
