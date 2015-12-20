using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public String Source { get; set; }
        public String Destination { get; set; }

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

            LinkActions.Add(new ViewAction("Odłącz", deactivateLink));
            LinkActions.Add(new ViewAction("Podłącz", activateLink));
            LinkActions.Add(new ViewAction("Usuń", deleteLink));

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
            //cloudPrcoess.CloseAll();
        }
        private bool isCreationReady()
        {
            return true;//selectedAction != null && selectedLink != null;
        }

        private void createLink()
        {
            StringBuilder msg = new StringBuilder();

            int src = processPortInput(msg, Source, "Źródło");
            int dst = processPortInput(msg, Destination, "Cel");
            if (msg.Length == 0)
            {
                /*List<int> invalid = cloudPrcoess.TryAddLink(src, dst);

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
                            msg.Append(invalid[0]);
                            msg.Append(" jest zajęty");
                            break;
                        }
                    case 2:
                        {
                            msg.Append("Porty : ");
                            msg.Append(invalid[0]);
                            msg.Append(" , ");
                            msg.Append(invalid[1]);
                            msg.Append(" są zajęte");
                            break;
                        }
                    default:
                        {
                            throw new Exception("Unexpected State of Application");
                        }
                  
           
                }
            */
            }

            ConsoleMessage = msg.ToString();
        }

        private int processPortInput(StringBuilder msg, String input, String portName)
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

        private void deleteLink()
        {
            SelectedLink.Stop();
            //cloudPrcoess.CloseLink(SelectedLink.GetModel());
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
