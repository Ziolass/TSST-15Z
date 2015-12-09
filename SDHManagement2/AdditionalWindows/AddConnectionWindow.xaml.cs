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

            handler = handler_;
            nodeNameLabel.Content = name;
            stringToPortArray(port_response);
            stringToConnectionArray(connection_response);

            
            
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
            connectionsBox.ItemsSource = connections.ToList();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int inport; 
            int outport;
            if ((int.TryParse(inportBox.SelectedItem.ToString(), out inport)) && int.TryParse(outportBox.SelectedItem.ToString(), out outport))
            {
                //sub-connection-HPC|{port_z1}#{port_do1}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}
                string command = "sub-connection-HPC|" + inport + "#" + outport + "#";
                handler.sendCommand(command, nodeNameLabel.Content.ToString(), true);
            }
                //TODO
            this.Close();
        }
    }
}
