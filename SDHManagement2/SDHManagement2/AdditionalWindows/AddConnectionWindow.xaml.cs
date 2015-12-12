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
        private string[] conteners = { "VC4", "VC3", "VC2", "VC12" };
        private int[] vc3levels = { 0,1,2};
        private int[] vc4levels = { 0 };
        private int[] vc2levels;
        private int []vc12levels;

        // 10 poniższych linii służy wyłączeniu 'x' w oknie
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        public AddConnectionWindow(SocketHandler handler_, string port_response, string connection_response,string name)
        {
            InitializeComponent();
            contenerTypeBox.ItemsSource = conteners.ToList();
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
                connections[i] = tmp[0] + " --> " + tmp[1];
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
                    string command = "sub-connection-HPC|" + inport + "#" + outport + "#" + fromlevel + "#" + tolevel + "#" + contenerTypeBox.SelectedItem.ToString();
                    handler.sendCommand(nodeNameLabel.Content.ToString(), command, true);

                    this.Close();
                }

                else
                {
                    MessageBox.Show("Values must be numeric only, try again");
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Values must be numeric only, try again");
                return;
            }
        }
        private void initArrays()
        {
            vc12levels = new int[21];
            vc2levels = new int[63];

            for(int i = 0; i < vc12levels.Length; i++)
            {
                vc12levels[i] = i;
            }
            for(int j =0; j< vc2levels.Length; j++)
            {
                vc2levels[j] = j;
            }
        }
        private void contenerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (contenerTypeBox.SelectedItem.ToString())
            {
                case "VC4":
                    endLevelBox.ItemsSource = vc4levels.ToList();
                    startLevelBox.ItemsSource = vc4levels.ToList();
                    break;
                case "VC3":
                    endLevelBox.ItemsSource = vc3levels.ToList();
                    startLevelBox.ItemsSource = vc3levels.ToList();
                    break;

                case "VC2":
                    endLevelBox.ItemsSource = vc2levels.ToList();
                    startLevelBox.ItemsSource = vc2levels.ToList();
                    break;
                case "VC12":
                    endLevelBox.ItemsSource = vc12levels.ToList();
                    startLevelBox.ItemsSource = vc12levels.ToList();
                    break;
                default:
                  
                    break;
            }
        }
    }
}
