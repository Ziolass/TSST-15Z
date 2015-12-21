using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Windows.Forms;
//using WireCloud;
using NetworkNode.SDHFrame;
using System.Collections.Specialized;


namespace SDHClient
{
    public partial class SDHClient : Form
    {

        public string nazwa = "";
        public Adaptation adapt;
        private int last_length = 0, last_listview_count = 0;
        LinkCollection lc;
        public static WindowsFormsSynchronizationContext mUiContext = new WindowsFormsSynchronizationContext();

        public string name
        {
            get
            {
                return nazwa;
            }
            set
            {
                nazwa = value;
            }
        }
        public SDHClient(string configurationFileName)
        {
            InitializeComponent();

            try
            {
                ConfigLoader configloader = new ConfigLoader(configurationFileName);
                lc = configloader.getLinks();
                this.textBox2.Text = configloader.Name;
                this.Show();
                this.label1.Text = "Port komunikacji z zarządzaniem:" + lc.management_port.ToString();
                adapt = new Adaptation(lc, configloader.Name, treeView1);
                //adapt.client_no = ;                //ostatecznie do wywalneia
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.Write("KLIENT: " + ex.Message);
            }


        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (last_listview_count != adapt.ports.Count)
            {
                listView1.Items.Clear();
                foreach (ListViewItem lvi in adapt.ports)
                    listView1.Items.Add(lvi);

                last_listview_count = adapt.ports.Count;
            }

            if (adapt.log.Count != last_length)
            {

                listBox1.Items.Clear();
                for (int a = adapt.log.Count - 1; a >= 0; a--)
                {
                    listBox1.Items.Add(adapt.log[a]);

                }
                last_length = adapt.log.Count;
            }
        }
        private void form_Resize(object sender, EventArgs e)
        {
            textBox1.Width = this.Width;
        }


        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && path != "")
            {
                //string port = listView1.Items[listView1.SelectedIndices[0]].Text;
                //try {

                string port = (path.Split('\\')[0]).Substring(path.Split('\\')[0].IndexOf(':') + 2, path.Split('\\')[0].Length - 2 - path.Split('\\')[0].IndexOf(':'));
                string lvl = (path.Split('\\')[1]).Substring(path.Split('\\')[1].IndexOf(':') + 1, path.Split('\\')[1].Length - 1 - path.Split('\\')[1].IndexOf(':'));

                string tmp = (path.Split('\\')[2]).Substring(path.Split('\\')[2].IndexOf(':') + 2, path.Split('\\')[2].Length - 2 - path.Split('\\')[2].IndexOf(':'));

                int lvl_no = Int32.Parse(tmp);
                VirtualContainerLevel vc = VirtualContainerLevel.UNDEF;
                switch (lvl)
                {
                    case "VC12": vc = VirtualContainerLevel.VC12; break;
                    case "VC32": vc = VirtualContainerLevel.VC32; break;
                    case "VC21": vc = VirtualContainerLevel.VC21; break;
                    case "VC4": vc = VirtualContainerLevel.VC4; break;

                }
                //int index = port.IndexOf(" ");
                // string substring = port.Substring(index);
                int port_no = Int32.Parse(port);

                foreach (IOPort io in lc.ports)
                {

                    if (io.PortTo == port_no)
                    {
                        //List<byte> bytes = new List<byte>();
                        //foreach (char c in textBox1.Text) bytes.Add((byte)c);
                        adapt.SendToRouter(port_no, Encoding.ASCII.GetBytes(textBox1.Text), vc, adapt.stmlvl, lvl_no, io.local_to);
                        break;
                    }
                }

                //} catch (Exception ex) { Console.WriteLine("coś poszło nie tak przy wysyłaniu. " + ex.Message); }

            }
            else if (listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Zaznacz jedno z łączy \"OUT\" w lewej dolnej części okna", "Nie zaznaczono łącza", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.Text = textBox2.Text;

        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            adapt.log.Clear();
        }

        private void SDHClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            //DISPOSE klas output i input - potrzebne?
        }
        private void listBox1_Click(object sender, EventArgs e)
        {
            if (sender != null && ((ListBox)sender).SelectedItem != null)
                MessageBox.Show((((ListBox)sender).SelectedItem).ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            Random r = new Random();
            for (int a = 0; a < 10; a++) textBox1.Text += (char)r.Next(40, 90);
        }

        string path = "";
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView tw = (TreeView)sender;
            path = tw.SelectedNode.FullPath.ToString();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = "";
            Random r = new Random();
            for (int a = 0; a < 100; a++) textBox1.Text += (char)r.Next(40, 90);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (path.Contains("OUT") && path.Split('\\').Length == 3)
            {
                toolStripStatusLabel2.Text = path;
                adapt.path_selected = path;
            }
            else
            {
                path = "";
                toolStripStatusLabel2.Text = "Zaznacz jedno z łączy OUT, kontener i szczelinę";
            }

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            adapt.log.Clear();
        }
    }
    public class FrameBusiness
    {
        public bool is_port_in;
        public Frame frame;
        public FrameBusiness(bool is_port_in, Frame f)
        {
            if (f == null)
                frame = new Frame();
            else
                frame = f;
            this.is_port_in = is_port_in;

        }
        public FrameBusiness() { }


    }
    public class ConnInfo
    {
        public int port_in;
        public int port_out;
        public int level_from;
        public int level_to;
        public VirtualContainerLevel level;
        public StmLevel stmlevel;
        public string identifier;
        static Random r;
        static public Dictionary<int, FrameBusiness> business = new Dictionary<int, FrameBusiness>();
        static List<string> used = new List<string>();

        public static List<int> levels_in = new List<int>();
        public static List<int> levels_out = new List<int>();
        public ConnInfo() { }
        public ConnInfo(int in_, int out_, int from, int to, VirtualContainerLevel level, StmLevel stm)
        {

            if (used.Count == 0) r = new Random(); //init tylko raz
            while (true)
            {
                identifier = "";
                identifier += (char)r.Next(65, 90);
                identifier += (char)r.Next(65, 90);
                bool exists = false;
                foreach (string us in used)
                    if (us == identifier) exists = true;

                if (exists == false) break;
                else Console.WriteLine(identifier);
            }
            used.Add(identifier);
            port_in = in_;
            port_out = out_;
            level_from = from;
            level_to = to;
            this.level = level;
            this.stmlevel = stm;
        }
    }
    public class Adaptation
    {
        public string path_selected = "";
        public string client_name;
        string to_decode = "";
        public List<string> log;
        public List<ListViewItem> ports;
        public List<ConnInfo> connections;
        public LinkCollection links;
        public TreeView tr;
        public StmLevel stmlvl;
        ClientManagementCenter cmc;
        ClientManagementPort mp;
        BackgroundWorker bw;
        public Adaptation(LinkCollection lc, string client_name, TreeView tr)
        {
            this.tr = tr;
            this.client_name = client_name;
            log = new List<string>();
            ports = new List<ListViewItem>();
            connections = new List<ConnInfo>();

            links = lc;
            //links.management_in.TurnOn();
            //links.management_in.HandleIncomingData += Management_in_HandleIncomingData;//////////////////////////////////////
            mp = new ClientManagementPort(links.management_port);

            cmc = new ClientManagementCenter(mp, this);




            //Console.WriteLine("TERAZ: RRELOC");
            //string s = cmc.PerformManagementAction("resource-relocation|3000#3001#0#0#VC4#STM1|3000#3006#1#1#VC4#STM1|3009#3008#1#1#VC32#STM1");


            // Console.WriteLine("TERAZ: GETCONS");
            // string d = cmc.PerformManagementAction("get-connections");
            // Console.WriteLine("TERAZ: GETPORTS");
            //  string ff = cmc.PerformManagementAction("get-ports");
            // string sss = cmc.PerformManagementAction("resource-relocation|3000#3001#0#0#VC32#STM1|3000#3006#1#1#VC32#STM1");


            // string gg = this.connections[0].identifier;

            //string dd = cmc.PerformManagementAction("delete-connection|"+gg);
            //string  gg1 = this.connections[0].identifier;
            // string dd2 = cmc.PerformManagementAction("delete-connection|" + gg1);


            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // if (client_no == "Klient1")Test( "resource - location |3021#3020#0#1#VC2");
            //   if(client_no == "Klient2") Test("resource - location |3020#3021#1#0#VC2"); //10  
            //////////////////////////////////////////////////////////////////////////////////////////////


            foreach (IOPort io in lc.ports)
            {
                io.HandleIncomingData += Input_HandleIncomingData;
                ports.Add(new ListViewItem(new string[] { "PORT: " + io.PortTo.ToString() }, null, Color.Black, Color.LightGreen, Control.DefaultFont));

            }



        }
        public void refreshConnections()
        {
            SDHClient.mUiContext.Post(refreshConnectionsThread, tr);

        }
        public void refreshConnectionsThread(object userData)
        {
            this.tr.Nodes.Clear();
            foreach (ConnInfo cn in this.connections)
            {
                int loc_in = -1, loc_out = -1;
                foreach (IOPort i in this.links.ports) if (i.listeningPort == cn.port_in) loc_in = i.local_listen;
                if (this.tr.Nodes.Find(cn.port_in.ToString(), false).Length == 0)
                {

                    this.tr.Nodes.Add(cn.port_in.ToString(), "(" + loc_in + ")IN: " + cn.port_in.ToString());
                }
                System.Windows.Forms.TreeNode tr;
                if (this.tr.Nodes.Find(cn.port_in.ToString(), false)[0].Nodes.Find(cn.level.ToString(), false).Length == 0)
                    tr = this.tr.Nodes.Find(cn.port_in.ToString(), false)[0].Nodes.Add(cn.level.ToString(), "CONTAINER:" + cn.level);
                else
                    tr = this.tr.Nodes.Find(cn.port_in.ToString(), false)[0].Nodes.Find(cn.level.ToString(), false)[0];
                tr.Nodes.Add(cn.level_from.ToString(), "LEVEL: " + cn.level_from.ToString());



                foreach (IOPort i in this.links.ports) if (i.PortTo == cn.port_out) loc_out = i.local_to;
                if (this.tr.Nodes.Find(cn.port_out.ToString(), false).Length == 0)
                {

                    this.tr.Nodes.Add(cn.port_out.ToString(), "(" + loc_out + ")OUT: " + cn.port_out.ToString());
                }
                System.Windows.Forms.TreeNode tr2;
                if (this.tr.Nodes.Find(cn.port_out.ToString(), false)[0].Nodes.Find(cn.level.ToString(), false).Length == 0)
                    tr2 = this.tr.Nodes.Find(cn.port_out.ToString(), false)[0].Nodes.Add(cn.level.ToString(), "CONTAINER:" + cn.level);
                else
                    tr2 = this.tr.Nodes.Find(cn.port_out.ToString(), false)[0].Nodes.Find(cn.level.ToString(), false)[0];
                tr2.Nodes.Add(cn.level_to.ToString(), "LEVEL: " + cn.level_to.ToString());
            }
        }
        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            mp.StartListening(cmc);
        }

        private string pass = "";
        // public bool received = false;
        private void Input_HandleIncomingData(object sender, EventArgs args)
        {
            IOPort i = (IOPort)sender;

            ConnInfo info = new ConnInfo();
            foreach (ConnInfo ci in this.connections) if (ci.port_in == i.listeningPort) info = ci;
            byte[] data = i.GetDataFromBuffer();

            //if (Encoding.ASCII.GetString(data).Substring(0, 2) != "OK") i.sendData(Encoding.ASCII.GetBytes("OK"));
            // else
            //  {
            //     received = true;
            //     Console.WriteLine(Encoding.ASCII.GetString(data).Substring(0, 2));
            // }
            int counter = 0;
            // List<char> chars = new List<char>();
            string str = pass;
            ////////////////////////////////////////////////////////////////////////////////////////////////////TODOwarunek sprawdzajacy zy zaladowano
            int a = 0;
            while (data[counter] != '\0' && data[counter + 1] != '\0' && data[counter + 2] != '\0' && data[counter + 3] != '\0')
            {
                byte c;
                int opn = 0, cls = 0;
                foreach (byte b in data)
                {
                    c = b;
                    if (b == '\0') { break; }
                    switch (c) { case 91: opn++; break; case 93: cls++; break; }
                    if (opn == 0) str += ((char)b).ToString();
                    else if (opn >= 1 && cls != opn)
                        str += ((char)b).ToString();
                    else if (opn >= 1 && cls == opn)
                    {


                        str += "]}";
                        string result = unpack(str, info.level, (byte)info.level_from);
                        //Encoding e = Encoding.GetEncoding("Windows-1250");
                        Encoding e = Encoding.ASCII;
                        //string res1 = e.GetString(Encoding.Convert(Encoding.Unicode, e, e.GetBytes(result)));
                        string res1 = e.GetString(e.GetBytes(result));
                        log.Add("Odebrano od: " + i.InputPort + " na poziomie " + info.level_from + ":" + res1 + "");
                        Console.WriteLine("Odebrano od: " + i.InputPort + ":  " + res1);
                        str = "";
                        break;
                    }
                    counter++;

                }
                if (opn != cls) { pass = str; break; }
            }



            to_decode = "";


        }
        public string unpack(string container, VirtualContainerLevel level, byte number)//pobiera kontener, zwraca czysty string
        {
            FrameBuilder fb = new FrameBuilder();
            Frame f = (Frame)fb.BuildFrame(container);
            //Frame result = new Frame();
            string data = "";
            
            //sobczakj
            ///TO CI ZAKOMENTOWAŁEM BO NIE DO KOŃCA WIEM OCB ALE CONTENT W VC JEST LISTA             
            //foreach (VirtualContainer vc in f.Content) if (vc != null && vc.Content.Content != null) data += vc.Content.Content;
            ///sobczakj

            //    if(f.Content[number] != null)
            // data += ((VirtualContainer)f.Content[number]).Content.Content.ToString();
            return data;

        }
        public List<Frame> pack(string raw_data, VirtualContainerLevel level, StmLevel stm, int number)
        {
            List<Frame> frames = new List<Frame>();
            int size = 0;
            number = Math.Abs(number); // nie moze byc ujemne
            // if (level == VirtualContainerLevel.VC12 && number > 62) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V12");
            //if (level == VirtualContainerLevel.VC21 && number > 21) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V2");
            //if (level == VirtualContainerLevel.VC32 && number > 2) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V3");
            // if (level == VirtualContainerLevel.VC4 && number > 1) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V4");

            if (level == VirtualContainerLevel.VC12)
            {
                size = 28;
            }
            if (level == VirtualContainerLevel.VC21)
            {
                size = 11 * 9;
            }
            if (level == VirtualContainerLevel.VC32)
            {
                size = 83 * 9;
            }
            if (level == VirtualContainerLevel.VC4)
            {
                size = 261;
            }
            else
            {
                size = 28;
            }
            // int index = raw_data.IndexOf('\0');
            // if (index == -1)
            int index = raw_data.Length;
            int no_frame = 0;
            int index1 = 0;
            FrameBuilder fbb = new FrameBuilder();
            frames.Add(new Frame(stm));



            for (int a = 0; (a < (Math.Ceiling((decimal)(index / (decimal)size)))); a++)
            {

                if (a > 0) frames.Add(new Frame(stm));
                VirtualContainer newVC = new VirtualContainer(level);
                if (index1 + size < raw_data.Length)
                {
                    newVC.SetContent(new NetworkNode.SDHFrame.Container(raw_data.Substring(index1, size))); //sobczakj  
                    index1 += size;
                    frames[no_frame].SetVirtualContainer(level, number, newVC);
                }
                else if (index1 < raw_data.Length)
                {
                    newVC.SetContent(new NetworkNode.SDHFrame.Container(raw_data.Substring(index1, raw_data.Length - index1))); //sobczakj
                    index1 += (raw_data.Length - index1);
                    frames[no_frame].SetVirtualContainer(level, number, newVC);
                }
                else
                {
                    //;do nothing
                    //wyjdzie w pętli i pójdzie do końca bo dane się skończyły
                }

                no_frame++;
                if (index1 >= raw_data.Length - 1) break;
            }


            return frames;
        }
        private static byte last_checksum = 0;
        public void SendToRouter(int port_to, byte[] raw_data, VirtualContainerLevel level, StmLevel stm, int level_no, int local_out)//raw_data jest stringiem w postaci ciągu bajtów, nie kontenerem
        {
            int current_container = 0;
            string data = "";

            ConnInfo info = new ConnInfo();
            foreach (ConnInfo ci in this.connections) if (ci.port_out == port_to) info = ci;
            byte number = (byte)info.level_to;
            foreach (byte b in raw_data)
            {
                char d = (char)b;
                if (d == '}' || d == '{' || d == '[' || d == ']') d = ' ';
                data += (char)d;
            }
            List<Frame> frames = new List<Frame>();
            try
            {
                frames = pack(data, info.level, info.stmlevel, level_no);
            }
            catch (IndexOutOfRangeException) { Console.WriteLine("Klient: Error: level o podanym numerze nie jest dostępny w ramce tego typu, zmien w menagerze"); return; }
            current_container++; data = ""; //osiągnięto koniec kontenera

            foreach (IOPort io in links.ports)
            {

                if (io.PortTo == port_to)
                {
                    foreach (Frame frame in frames)
                    {
                        //received = false;
                        //MD5 myHash = new MD5CryptoServiceProvider();
                        Frame frame2 = frame;
                        FrameBuilder fb = new FrameBuilder();
                        //List<byte> bytes_1 = new List<byte>();

                        string processed_data_2 = this.client_name + "|" + local_out + "|" + fb.BuildLiteral(frame);

                        //foreach (char c in processed_data_1)
                        //  {
                        // bytes_1.Add((byte)c);
                        //   }
                        // myHash.ComputeHash(bytes_1.ToArray<byte>());
                        // frame2.Msoh = Convert.ToBase64String(myHash.Hash).Substring(0, 1);
                        // frame2.Rsoh = last_checksum.ToString();
                        // last_checksum = (byte)Convert.ToByte(Convert.ToBase64String(myHash.Hash).Substring(0, 1)[0]);
                        FrameBuilder fb2 = new FrameBuilder();
                        //string processed_data_2 = fb.BuildLiteral(frame2);
                        List<byte> bytes_2 = new List<byte>();

                        foreach (char c in processed_data_2)
                        {

                            bytes_2.Add((byte)c);
                        }

                        io.sendData(bytes_2.ToArray<byte>());
                        //while (received==false) { }

                        Thread.Sleep(600); // nie ruszać! bez tego ramki wysyłają się na raz i blokuje się socket
                    }
                }
            }
        }
    }
}
