using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireCloud.CloudLogic;
using WireCloud.ViewModelUtils;

namespace WireCloud.ViewModels
{
    class TerminalViewModel : INotifyPropertyChanged
    {
        private CloudSetupProcess CloudSetupProcess;
        public ObservableCollection<LinkViewModel> Links { get; private set; }
        public ObservableCollection<ViewAction> LinkActions { get; private set; }

        public String SourcePort { get; set; }
        public String SourceNodeId { get; set; }
        public String SourceSocket { get; set; }
        public String DestinationPort { get; set; }
        public String DestinationNodeId { get; set; }
        public String DestinationSocket { get; set; }


        private String consoleMessage;

        public String ConsoleMessage
        {
            get
            {
                return consoleMessage;
            }
            set
            {
                consoleMessage = value;
                risePropertyChange(this, "ConsoleMessage");
            }
        }
        private LinkViewModel selectedLink;
        public LinkViewModel SelectedLink
        {
            get
            {
                return selectedLink;
            }
            set
            {
                selectedLink = value;
                risePropertyChange(this, "SelectedLink");
            }
        }
        private ViewAction selectedAction;
        public ViewAction SelectedAction
        {
            get
            {
                return selectedAction;
            }
            set
            {
                selectedAction = value;
                risePropertyChange(this, "SelectedAction");
            }
        }

        public ExternalCommand PerformLinkAction { get; private set; }
        public ExternalCommand CloseAllConections { get; private set; }
        public ExternalCommand CreateLink { get; private set; }
        public ExternalCommand DeleteLink { get; private set; }

        public TerminalViewModel()
        {
            CloudSetupProcess = new CloudSetupProcess("..\\..\\..\\Configs\\WireCloud\\cloudConfig.xml");
            CloudSetupProcess.LinksCreated += new LinksCreatedHandler(onLinksCreated);
            CloudSetupProcess.LinkCreated += new LinkCreatedHandler(onLinkCreated);
            CloudSetupProcess.StartCloudProcess();
            prepareLinkAction();
            PerformLinkAction = new ExternalCommand(performAction, areActionElementsReady);
            CloseAllConections = new ExternalCommand(performCloseConnections, isClosingReady);
            CreateLink = new ExternalCommand(createLink, isCreationReady);
            DeleteLink = new ExternalCommand(deleteLink, isLinkSelected);
        }

        private void onLinksCreated()
        {
            Links = new ObservableCollection<LinkViewModel>();

            foreach (Link link in this.CloudSetupProcess.Links)
            {
                Links.Add(new LinkViewModel(link));
            }
        }

        private void onLinkCreated(LinkCreatedArgs args)
        {
            Links.Add(new LinkViewModel(args.Link));
        }

        private void prepareLinkAction()
        {
            LinkActions = new ObservableCollection<ViewAction>();

            //LinkActions.Add(new ViewAction("Odłącz", deactivateLink));
            //LinkActions.Add(new ViewAction("Podłącz", activateLink));
            //LinkActions.Add(new ViewAction("Usuń", deleteLink));

        }
        private bool isLinkSelected()
        {
            return true;
            if (this.selectedLink != null)
                return true;
            else return false;
        }

        private bool areActionElementsReady()
        {
            return true;//selectedAction != null && selectedLink != null;
        }

        private void performAction()
        {
            if (selectedAction != null)
            {
                selectedAction.LinkAction();
            }
        }

        private bool isClosingReady()
        {
            return true;//selectedAction != null && selectedLink != null;
        }

        private void performCloseConnections()
        {
            this.CloudSetupProcess.StopCloudProccess();
        }
        private bool isCreationReady()
        {
            return true;//selectedAction != null && selectedLink != null;
        }

        private void createLink()
        {
            StringBuilder msg = new StringBuilder();

            int src_port = validatePortInput(msg, this.SourcePort, "Źródło - port");
            string src_nodeid = validateNodeIdInput(msg, this.SourceNodeId, "Źródło - nodeid");
            int src_socket = validatePortInput(msg, this.SourceSocket, "Źródło - socket");
            int dst_port = validatePortInput(msg, this.DestinationPort, "Cel - port");
            string dst_nodeid = validateNodeIdInput(msg, this.DestinationNodeId, "Cel - nodeid");
            int dst_socket = validatePortInput(msg, this.DestinationSocket, "Cel - socket");


            if (msg.Length == 0)
            {
                List<AbstractAddress> abstractAddress = new List<AbstractAddress>();
                abstractAddress.Add(new AbstractAddress(src_port, src_nodeid));
                abstractAddress.Add(new AbstractAddress(dst_port, dst_nodeid));
                List<NetworkNodeSender> networkNodeSender = new List<NetworkNodeSender>();
                networkNodeSender.Add(new NetworkNodeSender(src_socket));
                networkNodeSender.Add(new NetworkNodeSender(dst_socket));

                Dictionary<AbstractAddress, NetworkNodeSender> dictionary = new Dictionary<AbstractAddress, NetworkNodeSender>();

                if (abstractAddress[0].Equals(abstractAddress[1]) || networkNodeSender[0].Equals(networkNodeSender[1]))
                {
                    msg.Append("Źródło i cel są takie same");
                }
                else
                {
                    dictionary.Add(abstractAddress[0], networkNodeSender[1]);
                    dictionary.Add(abstractAddress[1], networkNodeSender[0]);


                    Link newLink = new Link(dictionary);

                    List<Link> invalid = this.CloudSetupProcess.TryAddLink(newLink);

                    switch (invalid.Count)
                    {
                        case 0:
                            {
                                msg.Append("Połączenie utworzone");
                                break;
                            }
                        case 1:
                            {
                                msg.Append("Port : ");
                                msg.Append(invalid[0].ToString());
                                msg.Append(" jest zajęty");
                                break;
                            }
                        case 2:
                            {
                                msg.Append("Porty : ");
                                msg.Append(invalid[0].ToString());
                                msg.Append(" , ");
                                msg.Append(invalid[1].ToString());
                                msg.Append(" są zajęte");
                                break;
                            }
                        default:
                            {
                                throw new Exception("Unexpected State of Application");
                            }
                    }
                }

            }

            ConsoleMessage = msg.ToString();
        }

        private int validatePortInput(StringBuilder msg, String input, String portName)
        {
            int result;
            if (input == null || input.Equals(""))
            {
                msg.Append("Wprowadź ");
                msg.Append(portName);
                msg.Append("\n");
            }
            if (!int.TryParse(input, out result))
            {
                msg.Append(portName);
                msg.Append(" nie jest liczbą");
                msg.Append("\n");
            }

            return result;
        }
        private string validateNodeIdInput(StringBuilder msg, String input, String portName)
        {
            string result = string.Empty;
            if (input == null || input.Equals(""))
            {
                msg.Append("Wprowadź ");
                msg.Append(portName);
                msg.Append("\n");
            }
            else result = input;
            return result;
        }

        /// <summary>
        /// Deletes the link.
        /// </summary>
        public void deleteLink()
        {
            this.CloudSetupProcess.ProcessMonitor.DeleteLink(SelectedLink.GetModel());
            Links.Remove(SelectedLink);
            SelectedLink = null;
        }

        private void deactivateLink()
        {
            SelectedLink.Active = false;
            risePropertyChange(SelectedLink, "Active");
        }

        private void activateLink()
        {
            SelectedLink.Active = true;
            risePropertyChange(SelectedLink, "Active");
        }

        private void risePropertyChange(object sender, String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(property));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
