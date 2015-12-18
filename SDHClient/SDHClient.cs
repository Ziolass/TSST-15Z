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
using WireCloud;
using NetworkNode.SDHFrame;
using System.Security.Cryptography;

namespace SDHClient
{
    public partial class SDHClient : Form
    {
       
        public string nazwa = "";
        public Adaptation adapt;
        private int last_length = 0,last_listview_count=0;
        LinkCollection lc;
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
           
            ConfigLoader configloader = new ConfigLoader(configurationFileName);
            lc = configloader.getLinks();
            this.textBox2.Text = configloader.Name;
            this.Show();
            this.label1.Text = "Port komunikacji z zarządzaniem:" +lc.management_port.ToString();
            adapt = new Adaptation(lc,configloader.Name);
            //adapt.client_no = ;                //ostatecznie do wywalneia
            timer1.Enabled = true;

        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(last_listview_count != adapt.ports.Count)
            {
                listView1.Items.Clear();
                foreach (ListViewItem lvi in adapt.ports)
                    listView1.Items.Add(lvi);

                last_listview_count = adapt.ports.Count;
            }

            if (adapt.log.Count != last_length)
            {
                
                listBox1.Items.Clear();
                for (int a = adapt.log.Count - 1; a >= 0; a--) { 
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
            if (e.KeyCode == Keys.Enter&&listView1.SelectedIndices.Count != 0)
            {
                string port = listView1.Items[listView1.SelectedIndices[0]].Text;
                int index = port.IndexOf(" ");
                string substring = port.Substring(index);
                int port_no = Int32.Parse(substring);
                
                foreach (IOPort io in lc.ports)
                {
                    
                    if(io.PortTo == port_no)
                    {
                        //List<byte> bytes = new List<byte>();
                        //foreach (char c in textBox1.Text) bytes.Add((byte)c);
                        adapt.SendToRouter(port_no, Encoding.Unicode.GetBytes(textBox1.Text)); //TODO   
                        break;
                    }
                }

            }
            else if(listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Zaznacz jedno z łączy \"OUT\" w prawej dolnej części okna","Nie zaznaczono łącza",MessageBoxButtons.OK,MessageBoxIcon.Information);
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
            if(sender != null && ((ListBox)sender).SelectedItem != null)
            MessageBox.Show((((ListBox)sender).SelectedItem).ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            Random r = new Random();
            for (int a = 0; a < 10; a++) textBox1.Text += (char)r.Next(40, 90);
        }

        
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

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
            toolStripStatusLabel2.Text = adapt.received.ToString() + "                          ";
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            adapt.log.Clear();
        }
    }

    public class ConnInfo
    {
       public  int port_in;
      public   int port_out;
        public int level_from;
      public   int level_to;
     public    VirtualContainerLevel level;
        public StmLevel stmlevel;
        
        public ConnInfo() { }
        public ConnInfo(int in_,int out_,int from, int to, VirtualContainerLevel level,StmLevel stm)
        {
            port_in = in_;
            port_out= out_;
            level_from = from;
            level_to = to;
            this.level = level;
            this.stmlevel = stm;
        }
    }
    public class Adaptation
    {
        public string client_name;
        string to_decode = "";
        public List<string> log;
        public List<ListViewItem> ports;
        public List<ConnInfo> connections;
       public  LinkCollection links;
        ClientManagementCenter cmc;
        ClientManagementPort mp;
        BackgroundWorker bw;
        public Adaptation(LinkCollection lc,string client_name)
        {
            
            this.client_name = client_name;
            log = new List<string>();
            ports = new List<ListViewItem>();
            connections = new List<ConnInfo>();
            links = lc;
            //links.management_in.TurnOn();
            //links.management_in.HandleIncomingData += Management_in_HandleIncomingData;//////////////////////////////////////
            mp = new ClientManagementPort(links.management_port);
            cmc = new ClientManagementCenter(mp, this);
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

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            mp.StartListening(cmc);
        }
        
        private string pass = "";
        public bool received = false;
        private void Input_HandleIncomingData(object sender, EventArgs args)
        {
            IOPort i = (IOPort)sender;
            
            ConnInfo info = new ConnInfo();
            foreach (ConnInfo ci in this.connections) if (ci.port_in == i.listeningPort) info = ci;
            byte[] data = i.GetDataFromBuffer();

            if (Encoding.ASCII.GetString(data).Substring(0, 2) != "OK") i.sendData(Encoding.ASCII.GetBytes("OK"));
            else
            {
                received = true;
                Console.WriteLine(Encoding.ASCII.GetString(data).Substring(0, 2));
            }
            int counter = 0;
            // List<char> chars = new List<char>();
            string str = pass;
            ////////////////////////////////////////////////////////////////////////////////////////////////////TODOwarunek sprawdzajacy zy zaladowano
            int a = 0;
            while (data[counter] != '\0'&&data[counter+1]!= '\0'&&data[counter+2]!='\0'&&data[counter+3]!='\0')

            {
                byte c;
                int opn = 0, cls = 0;
                foreach (byte b in data)
                {
                    c = b;
                    if (b == '\0') {  break; }
                    switch (c) { case 91: opn++; break; case 93: cls++; break; }
                    if (opn == 0) str += ((char)b).ToString();
                    else if (opn >= 1 && cls != opn)
                        str += ((char)b).ToString();
                    else if (opn >= 1 && cls == opn)
                    {

                        
                        str += "]}";
                        string result = unpack(str, info.level, (byte)info.level_from);
                        Encoding e = Encoding.GetEncoding("Windows-1250");

                        string res1 = e.GetString(Encoding.Convert(Encoding.Unicode, e, e.GetBytes(result)));
                        log.Add("Odebrano od: " + i.InputPort + " na poziomie " + info.level_from + ":" +res1 + "");
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
        public string unpack(string container,VirtualContainerLevel level, byte number)//pobiera kontener, zwraca czysty string
        {
            FrameBuilder fb = new FrameBuilder();
            Frame f = (Frame)fb.BuildFrame(container);
            //Frame result = new Frame();
            string data = "";
            foreach (VirtualContainer vc in f.Content) if(vc != null && vc.Content.Content != null) data += vc.Content.Content; 
        //    if(f.Content[number] != null)
           // data += ((VirtualContainer)f.Content[number]).Content.Content.ToString();
            return data;

        }
        public List<Frame> pack(string raw_data, VirtualContainerLevel level,StmLevel stm, byte number) {
            List<Frame> frames = new List<Frame>();
            int size = 0;
            number = (byte)Math.Abs(number); // nie moze byc ujemne
           // if (level == VirtualContainerLevel.VC12 && number > 62) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V12");
            //if (level == VirtualContainerLevel.VC21 && number > 21) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V2");
            //if (level == VirtualContainerLevel.VC32 && number > 2) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V3");
           // if (level == VirtualContainerLevel.VC4 && number > 1) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V4");

            if (level == VirtualContainerLevel.VC12) {
                size = 28;
            }
            if (level == VirtualContainerLevel.VC21) {
                size = 11*9 ;
            }
            if (level == VirtualContainerLevel.VC32)
            {
                 size = 83*9 ;
            }
            if (level == VirtualContainerLevel.VC4) {
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
            


                for (int a = 0; (a < ( Math.Ceiling((decimal)(index / (decimal)size)))); a++)
                {

                if (a > 0) frames.Add(new Frame(stm));
                    VirtualContainer newVC = new VirtualContainer(level);
                    if (index1 + size < raw_data.Length)
                    {
                        newVC.Content = new NetworkNode.SDHFrame.Container(raw_data.Substring(index1, size));
                        index1 += size;
                        frames[no_frame].SetVirtualContainer(level, number, newVC);
                    }
                    else if (index1 < raw_data.Length)
                    {
                        newVC.Content = new NetworkNode.SDHFrame.Container(raw_data.Substring(index1, raw_data.Length - index1));
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
        private static byte last_checksum =0;
        public void SendToRouter(int port_to, byte[] raw_data)//raw_data jest stringiem w postaci ciągu bajtów, nie kontenerem
        {
            int current_container=0;
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
            List<Frame> frames  = new List<Frame>();
            try {frames = pack(data, info.level,info.stmlevel, number);
            }
            catch(IndexOutOfRangeException) { Console.WriteLine("Klient: Error: level o podanym numerze nie jest dostępny w ramce tego typu, zmien w menagerze"); return; }
            current_container++; data = ""; //osiągnięto koniec kontenera

            foreach (IOPort io in    links.ports)
            {

                if (io.PortTo == port_to)
                {
                    foreach (Frame frame in frames)
                    {
                        received = false;
                        MD5 myHash = new MD5CryptoServiceProvider();
                        Frame frame2 = frame;
                        FrameBuilder fb = new FrameBuilder();
                        List<byte> bytes_1 = new List<byte>();

                        string processed_data_1 = fb.BuildLiteral(frame);

                        foreach (char c in processed_data_1)
                        {
                            bytes_1.Add((byte)c);
                        }
                        myHash.ComputeHash(bytes_1.ToArray<byte>());
                       // frame2.Msoh = Convert.ToBase64String(myHash.Hash).Substring(0, 1);
                       // frame2.Rsoh = last_checksum.ToString();
                        last_checksum = (byte)Convert.ToByte(Convert.ToBase64String(myHash.Hash).Substring(0, 1)[0]);
                        FrameBuilder fb2 = new FrameBuilder();
                        string processed_data_2 = fb.BuildLiteral(frame2);
                        List<byte> bytes_2 = new List<byte>();

                        foreach (char c in processed_data_2)
                        {

                            bytes_2.Add((byte)c);
                        }
                        
                        io.sendData(bytes_2.ToArray<byte>());
                        while (received==false) { }
                        Thread.Sleep(600); // nie ruszać! bez tego ramki wysyłają się na raz i blokuje się socket
                    }
                }
            }
        }



    }
   
}
