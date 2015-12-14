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
            "close-connection",
            "sub-connection-HPC",
            "get-connection-list",
            "get-ports"
        };
        // Krosownica
        private string [] actionaNameList =
            {"Dezaktywuj krosownice",
            "Usuń istniejące połączenie",
            "Dodaj nowe połączenie",
            "Pobierz listę istniejących połączeń",
            "Pobierz listę dostępnych portów"
        };
        private string[] clientAction =
            {"resource-location"};
        private string[] clientNameAction =
            {"Przydział zasobów" };

        private string[] selection =
            {
             "Krosownica",
             "Klient"
            };

        public List<Router> routerList { get; set; }
        public List<Router> clientList { get; set; }
        private bool initialised = false;

        public MainWindow()
        {
            InitializeComponent();
            selectionBox.IsEnabled = false;
            actionBox.IsEnabled = false;
            nodeBox.IsEnabled = false;

            console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt") + ": Konsola zarządzania");
            selectionBox.ItemsSource = selection.ToList();
            actionBox.ItemsSource = actionaNameList.ToList();
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
            if (selectionBox.SelectedIndex == 0)
            {
                action = actionList[actionBox.SelectedIndex];
            }
            else
            {
                action = clientAction[actionBox.SelectedIndex];
            }

            //action = actionList[actionBox.SelectedIndex];
            socketHandler.commandHandle(action,nodeName);
            
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
                                    +": Dostępne porty"
                                    + "\nWejściowe: " + inports
                                    + "\nWyjściowe: " + outports);
                

            }
            else if (command.Equals("get-connection-list|"))
            {
                string[] tmp = stringToConnectionArray(text);


                console.Items.Add(DateTime.Now.ToString("HH:mm:ss tt")
                                    + ":\n"
                                    + name
                                    + " Istniejące połączenia");
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
                int position = i + 1;
                connections[i] = "Polaczanie "+position+".\n"+
                    "z: "+tmp[0] + " do " + tmp[1]+"\n"+
                    "z pozycji "+tmp[2]+". na pozycje "+tmp[3]+".\n"+
                    "obsługiwany kontener: "+tmp[4]+
                    "moduł: " + tmp[5];
            }
            return connections;
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            List<int> portList = ConfigReader.readPortsFromConfig("\\managementConfigFile.xml");
            routerList = new List<Router>();
            clientList = new List<Router>();

            
            if (portList==null)
            {
                appendConsole("Błąd odczytu pliku konfiguracyjnego, spróbuj ponownie.",null,null);
                return;
            }

            socketHandler = new SocketHandler(portList, this);
            actionBox.IsEnabled = true;
            nodeBox.IsEnabled = true;
            selectionBox.IsEnabled = true;
            button1.IsEnabled = false;
            button2.IsEnabled = true;
            addNewButton.IsEnabled = true;
            initialised = true;


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

        private void nodeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void selectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if(!initialised)
            {
                nodeBox.ItemsSource = new List<string>();
                return;

            }
            switch (selectionBox.SelectedItem.ToString())
            {
                case "Klient":
                   nodeBox.ItemsSource = socketHandler.clientNameList;

                    actionBox.ItemsSource = clientNameAction.ToList();
                    break;
                case "Krosownica" :
                    nodeBox.ItemsSource = socketHandler.nodelist;
                    actionBox.ItemsSource = actionaNameList.ToList();
                    break;
                default:
                    break;
            }
        }

        private void actionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
