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
    /// Interaction logic for ResourceRelocationWindow.xaml
    /// </summary>
    public partial class ResourceRelocationWindow : Window
    {
        private SocketHandler handler;
        private string[] connections;
        private string[] inports;
        private string[] outports;
        private string[] conteners = { "VC4", "VC32", "VC21", "VC12" };
        private string[] modules = { "STM1", "STM4", "STM16", "STM64", "STM256" };
        private int[] modulesInt = { 1, 4, 16, 64, 256 };

        private int[] vc32levels = { 0, 1, 2 };
        private int[] vc4levels = { 0 };
        private int[] vc21levels;
        private int[] vc12levels;

        public ResourceRelocationWindow(SocketHandler handler_, string name,string port_response)
        {
            InitializeComponent();
            contenerTypeBox.ItemsSource = conteners.ToList();
            ModuleComboBox.ItemsSource = modules.ToList();
            handler = handler_;
            stringToPortArray(port_response);

            nodeNameLabel.Content = name;
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
            connections = new string[temp_connections.Length];

            for (int i = 0; i < temp_connections.Length; i++)
            {
                string[] tmp = temp_connections[i].Split('#');
                connections[i] = tmp[0] + " --> " + tmp[1];
            }

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
                    string command = "resource-location|" + inport + "#" + outport + "#" + fromlevel + "#" + tolevel + "#" + contenerTypeBox.SelectedItem.ToString()+"#"+ModuleComboBox.SelectedItem.ToString();
                    handler.sendCommand(nodeNameLabel.Content.ToString(), command, true);

                    this.Close();
                }

                else
                {
                    MessageBox.Show("Dopuszczalne są tylko wartości numeryczne");
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Dopuszczalne są tylko wartości numeryczne");
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

            int SMIdentifier = modulesInt[ModuleComboBox.SelectedIndex];

            vc12levels = new int[63 * SMIdentifier];

            for (int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }

        }
        private void reInitvc21()
        {
            int SMIdentifier = modulesInt[ModuleComboBox.SelectedIndex];
            vc21levels = new int[21 * SMIdentifier];

            for (int j = 0; j < vc21levels.Length; j++)
            {
                vc21levels[j] = j;
            }
        }
        private void reInitvc32()
        {
            int SMIdentifier = modulesInt[ModuleComboBox.SelectedIndex];
            vc32levels = new int[3 * SMIdentifier];


            for (int j = 0; j < vc32levels.Length; j++)
            {
                vc32levels[j] = j;
            }
        }
        private void reInitvc4()
        {
            int SMIdentifier = modulesInt[ModuleComboBox.SelectedIndex];
            vc4levels = new int[1 * SMIdentifier];


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
                    endLevelBox.ItemsSource = vc4levels.ToList();
                    startLevelBox.ItemsSource = vc4levels.ToList();
                    break;
                case "VC31":
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
    }
}
