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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using SDHManagement2.FileUtils;
using SDHManagement2.Models;
using SDHManagement2.SocketUtils;
using SDHManagement2.AdditionalWindows;

namespace SDHManagement2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private SocketHandler socketHandler;

        private string[] actionList = 
            {"disable-node",
            "shutdown-interface",
            "sub-connection-HPC",
            "get-connection-list",
            "get-ports"
        };

        public List<Router> routerList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt") + ": Management console");
            actionBox.ItemsSource = actionList.ToList();
            button2.IsEnabled = false;
            addNewButton.IsEnabled = false;
        //    socketHandler = new SocketHandler(routerList, this);

            /*ZoomControl.SetViewFinderVisibility(zoomctrl,Visibility.Visible);
            zoomctrl.ZoomToFill();

            GraphAreaExample_Setup();
            init();*/
        }

        /*
        void init()
        {
           
            Area.GenerateGraph(true,true);

           // Area.SetEdgesDashStyle(EdgeDashStyle.Dash);

            Area.ShowAllEdgesArrows(true);
            Area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(1));
            Area.ShowAllEdgesLabels(false);

            zoomctrl.ZoomToFill();
        }

        private void GraphAreaExample_Setup()
        {
            var logicCore = new GXLogicCoreExample() {Graph = GraphExample_Setup()};

            logicCore.DefaultLayoutAlgorithm =LayoutAlgorithmTypeEnum.KK;

            ((KKLayoutParameters) logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;

            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            logicCore.DefaultEdgeRoutingAlgorithm=EdgeRoutingAlgorithmTypeEnum.SimpleER;

            logicCore.AsyncAlgorithmCompute = false;

            Area.LogicCore = logicCore;


        }

        private GraphExample GraphExample_Setup()
        {
            var dataGraph = new GraphExample();

            for (int i = 0; i < 5; i++)
            {
                var dataVertex = new DataVertex("Router:"+i);
                dataGraph.AddVertex(dataVertex);

            }

            var vlist = dataGraph.Vertices.ToList();
            var dataEdge = new DataEdge(vlist[0], vlist[1]);// {Text =string.Format("{0} -> {1}",vlist[0],vlist[1])};
            dataGraph.AddEdge(dataEdge);
            dataEdge = new DataEdge(vlist[2], vlist[3]);//{ Text = string.Format("{0} -> {1}", vlist[2], vlist[3]) };
            dataGraph.AddEdge(dataEdge);
            return dataGraph;

        }
        */
        public void Dispose()
        {
           // Area.Dispose(); 
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string nodeName;
            string action;

            nodeName = nodeBox.Text;
            action = actionBox.Text;

            socketHandler.commandHandle(action,nodeName);


            //if(socketHandler.sendCommand(nodeName, action) == null) { return; }



        }

        public void appendConsole(string text, string name, string command)
        {
            if (command == null || name == null)
            {
                console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt") + ": " + text);
                
            }

            else if (command.Equals("get-ports|"))
            {
                string[] tmp = stringToPortArray(text);
                string inports = tmp[0];
                string outports = tmp[1];
                console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt")
                                    + ":\n"
                                    +name
                                    +"'s available ports"
                                    + "\nIn: " + inports
                                    + "\nOut: " + outports);
                

            }
            else if (command.Equals("get-connection-list|"))
            {
                string[] tmp = stringToConnectionArray(text);


                console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt")
                                    + ":\n"
                                    + name
                                    + "'s current connections");
                foreach(string con in tmp)
                {
                    console.Items.Add(con);
                }
            }
            else
            {
                console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt") + ": " + name+": "+text);
            }

            console.SelectedIndex = console.Items.Count - 1;
            console.ScrollIntoView(console.SelectedItem);
        }
        private string [] stringToPortArray(String port_string)
        {
            string[] temp_ports = port_string.Split('|');
            string [] inports = temp_ports[0].Split('#');
            string [] outports = temp_ports[1].Split('#');

            string[] result = new string[2];
            result[0] = string.Join(", ", inports);
            result[1] = string.Join(", ", outports);

            return result;

        }

        private string [] stringToConnectionArray(String con_string)
        {
            string[] temp_connections = con_string.Split('|');
            string [] connections = new string[temp_connections.Length];

            for (int i = 0; i < temp_connections.Length; i++)
            {
                string[] tmp = temp_connections[i].Split('#');
                connections[i] = tmp[0] + " --> " + tmp[1];
            }
            return connections;
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {

            Dictionary<string, int> portDictionary = ConfigReader.readConfig("\\managementConfigFile.xml");
            routerList = new List<Router>();

            
            if (portDictionary==null)
            {
                appendConsole("Error reading configuration file. Try again",null,null);
                return;
            }

            foreach (var VARIABLE in portDictionary)
            {
                Router router = new Router()
                {
                    identifier = VARIABLE.Key,
                    connected = false,
                    port = VARIABLE.Value
                };
                routerList.Add(router);

            }

            socketHandler = new SocketHandler(routerList, this);
            button1.IsEnabled = false;
            button2.IsEnabled = true;
            addNewButton.IsEnabled = true;



        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            socketHandler.refresh();
        }
        private void addNewButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewRouterWindow newRouter = new AddNewRouterWindow(this,socketHandler);
            newRouter.ShowDialog();
            return;

        }
    }
}
