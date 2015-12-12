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
using NetworkNode.Ports;
using NetworkNode.Frame;
using System.Security.Cryptography;

namespace Client
{
    public partial class SDHClient : Form
    {
       
        private string nazwa = "";
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
                
                foreach (KeyValuePair<int, Output> output in lc.output_ports)
                {
                    
                    if(output.Value.Port == port_no)
                    {
                        List<byte> bytes = new List<byte>();
                        foreach (char c in textBox1.Text) bytes.Add((byte)c);
                        adapt.SendToRouter(port_no, bytes.ToArray<byte>()); //TODO   
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
            MessageBox.Show((((ListBox)sender).SelectedItem).ToString());
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
        public ConnInfo() { }
        public ConnInfo(int in_,int out_,int from, int to, VirtualContainerLevel level)
        {
            port_in = in_;
            port_out= out_;
            level_from = from;
            level_to = to;
            this.level = level;
        }
    }
    public class Adaptation
    {
        string to_decode = "";
        public List<string> log;
        public List<ListViewItem> ports;
        public List<ConnInfo> connections;
        LinkCollection links;
        
        public Adaptation(LinkCollection lc,string client_no)
        {
            log = new List<string>();
            ports = new List<ListViewItem>();
            connections = new List<ConnInfo>();
            links = lc;
            links.management_in.TurnOn();
            links.management_in.HandleIncomingData += Management_in_HandleIncomingData;//////////////////////////////////////


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if(client_no == "Klient1")Test( "resource - location |3021#3020#0#1#VC2");
            if(client_no == "Klient2") Test("resource - location |3020#3021#1#0#VC2"); //10  
            //////////////////////////////////////////////////////////////////////////////////////////////


            foreach (Input input in lc.input_ports)
            {
                //input.TurnOn(); //wywala wyjątki
                input.HandleIncomingData += Input_HandleIncomingData;
                ports.Add(new ListViewItem(new string[] { "IN: " + input.InputPort.ToString() }, null, Color.Black, Color.LightGreen, Control.DefaultFont));

            }
            foreach (KeyValuePair<int, Output> output in lc.output_ports)
            {
                ports.Add(new ListViewItem(new string[] { "OUT: " + output.Value.Port.ToString() }, null, Color.Black, Color.LightGoldenrodYellow, Control.DefaultFont));


            }


        }
        private void Test(string str)
        {
            
            
            bool ok = false;
            int index1 = str.IndexOf('|');
            int index2 = str.IndexOf('#', index1);
            int index3 = str.IndexOf('#', index2+1);
            int index4 = str.IndexOf('#', index3+1);
            int index5 = str.IndexOf('#', index4+1);
            int index6 = str.IndexOf('#', index5+1);
            if (index1 != -1 && index2 != -1  && index3 != -1  && index4 != -1 &&  index5 != -1 && index6 != -1)ok = true;
            try
            {
                int port_in = Int32.Parse(str.Substring(index1 + 1, index2 - (index1 + 1)));
                int port_out = Int32.Parse(str.Substring(index2 + 1, index3 - (index2 + 1)));
                int from = Int32.Parse(str.Substring(index3 + 1, index4 - (index3 + 1)));
                int to = Int32.Parse(str.Substring(index4 + 1, index5 - (index4 + 1)));
                VirtualContainerLevel vc = VirtualContainerLevel.VC12; ;
                string level = str.Substring(index5 + 1, str.Length - (index5 + 1));
                switch (level)
                {
                    case "VC12": vc = VirtualContainerLevel.VC12; break;
                    case "VC3": vc = VirtualContainerLevel.VC3; break;
                    case "VC2": vc = VirtualContainerLevel.VC2; break;
                    case "VC4": vc = VirtualContainerLevel.VC4; break;

                }

                connections.Add(new ConnInfo(port_in, port_out, from, to, vc));
            }
            catch (Exception) { ok = false; }



        }
        private void Management_in_HandleIncomingData(object sender, EventArgs args)
        {
            //resource-location|{port_we}#{port_wy}#{poziom_z1}#{poziom_do1}#{typ_konteneru1}
            //                12      2e  3     3e  4        4e 5
            bool ok = false;
            Input i = (Input)sender;
            string str = "";
            byte[] data = i.GetDataFromBuffer();
            foreach (byte b in data) str += (char)b;


            if (str.Contains("get") && str.Contains("ports")){
                Console.WriteLine("Klient: management żąda: get ports");
                string to_send = "";
                foreach(Input ii in links.input_ports) { to_send += ii.InputPort; to_send += "#"; }
                to_send.Remove(to_send.Length - 1, 1);
                to_send += '|';
                foreach (KeyValuePair<int,Output> kv in links.output_ports) { to_send +=kv.Value.Port; to_send += "#"; }
            }
            else
            {
                try
                {
                    int index1 = str.IndexOf('|');
                    int index2 = str.IndexOf('#', index1);
                    int index3 = str.IndexOf('#', index2 + 1);
                    int index4 = str.IndexOf('#', index3 + 1);
                    int index5 = str.IndexOf('#', index4 + 1);
                    int index6 = str.IndexOf('#', index5 + 1);
                    if (index1 != -1 && index2 != -1 && index3 != -1 && index4 != -1 && index5 != -1 && index6 != -1) ok = true;

                    int port_in = Int32.Parse(str.Substring(index1 + 1, index2 - (index1 + 1)));
                    int port_out = Int32.Parse(str.Substring(index2 + 1, index3 - (index2 + 1)));
                    int from = Int32.Parse(str.Substring(index3 + 1, index4 - (index3 + 1)));
                    int to = Int32.Parse(str.Substring(index4 + 1, index5 - (index4 + 1)));
                    VirtualContainerLevel vc = VirtualContainerLevel.VC12; ;
                    string level = str.Substring(index5 + 1, str.Length - (index5 + 1));
                    switch (level)
                    {
                        case "VC12": vc = VirtualContainerLevel.VC12; break;
                        case "VC3": vc = VirtualContainerLevel.VC3; break;
                        case "VC2": vc = VirtualContainerLevel.VC2; break;
                        case "VC4": vc = VirtualContainerLevel.VC4; break;

                    }

                    connections.Add(new ConnInfo(port_in, port_out, from, to, vc));
                    Console.WriteLine("Klient: przyjęto port_in{0}, port_out{1}, poziom_z{2},poziom_do{3},kontener{4} ", port_in, port_out, from, to, level);
                }
                catch (Exception) { ok = false; }

                if (ok)
                {
                    string ok_ = "OK";
                    List<byte> b = new List<byte>();
                    foreach (char c in ok_) b.Add((byte)c);
                    links.management_out.sendData(b.ToArray<byte>());
                }
                else
                {
                    string notok_ = "ERROR";
                    List<byte> b = new List<byte>();
                    foreach (char c in notok_) b.Add((byte)c);
                    links.management_out.sendData(b.ToArray<byte>());
                }
            }
        }
        

        private string pass = "";
        private void Input_HandleIncomingData(object sender, EventArgs args)
        {
            Input i = (Input)sender;
            ConnInfo info = new ConnInfo();
            foreach (ConnInfo ci in this.connections) if (ci.port_in == i.InputPort) info = ci;
            byte[] data = i.GetDataFromBuffer();
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
                        
                        log.Add("Odebrano od: " + i.InputPort + ":  " + unpack(str,info.level,(byte)info.level_from));
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
            SDHFrame f = (SDHFrame)fb.BuildFrame(container);
            //Frame result = new Frame();
            string data = "";
            foreach (VirtualContainer vc in f.Content) if(vc != null && vc.Content.Content != null) data += vc.Content.Content; 
        //    if(f.Content[number] != null)
           // data += ((VirtualContainer)f.Content[number]).Content.Content.ToString();
            return data;

        }
        public List<SDHFrame> pack(string raw_data, VirtualContainerLevel level, byte number) {
            List<SDHFrame> frames = new List<SDHFrame>();
            int size = 0;
            number = (byte)Math.Abs(number); // nie moze byc ujemne
            if (level == VirtualContainerLevel.VC12 && number > 62) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V12");
            if (level == VirtualContainerLevel.VC2 && number > 21) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V2");
            if (level == VirtualContainerLevel.VC3 && number > 2) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V3");
            if (level == VirtualContainerLevel.VC4 && number > 1) throw new IndexOutOfRangeException("Przekroczono zakres poziomu kontenera V4");

            ContainerLevel cl = new ContainerLevel();
            if (level == VirtualContainerLevel.VC12) {
                cl = ContainerLevel.TUG12; size = 28;
            }
            if (level == VirtualContainerLevel.VC2) {
                cl = ContainerLevel.TUG2; size = 11*9 ;
            }
            if (level == VirtualContainerLevel.VC3)
            {
                cl = ContainerLevel.TUG3; size = 83*9 ;
            }
            if (level == VirtualContainerLevel.VC4) {
                cl = ContainerLevel.AU4; size = 261;
            }

            int index = raw_data.IndexOf('\0');
            if (index == -1) index = raw_data.Length;
            int no_frame = 0;
            int index1 = 0;
           
                
                for (int a = 0; (a < ( Math.Ceiling((decimal)(index / (decimal)size)))); a++)
                {
                    frames.Add(new SDHFrame());
                    VirtualContainer newVC = new VirtualContainer(level);
                    if (index1 + size < raw_data.Length)
                    {
                        newVC.Content = new NetworkNode.Frame.Container(raw_data.Substring(index1, size));
                        index1 += size;
                        frames[no_frame].SetVirtualContainer(cl, number, newVC);
                    }
                    else if (index1 < raw_data.Length)
                    {
                        newVC.Content = new NetworkNode.Frame.Container(raw_data.Substring(index1, raw_data.Length - index1));
                        index1 += (raw_data.Length - index1);
                        frames[no_frame].SetVirtualContainer(cl, number, newVC);
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
        public void SendToRouter(int port, byte[] raw_data)//raw_data jest stringiem w postaci ciągu bajtów, nie kontenerem
        {
            int current_container=0;
            string data = "";

            ConnInfo info = new ConnInfo();
            foreach (ConnInfo ci in this.connections) if (ci.port_out == port) info = ci;
            byte number = (byte)info.level_to;
            foreach (byte b in raw_data)
            {
                char d = (char)b;
                if (d == '}' || d == '{' || d == '[' || d == ']') d = ' ';
                data += (char)d;
            }
            List<SDHFrame> frames  = new List<SDHFrame>();
            try {frames = pack(data, info.level, number);
            }
            catch(IndexOutOfRangeException) { Console.WriteLine("Klient: Error: level o podanym numerze nie jest dostępny w ramce tego typu, zmien w menagerze"); return; }
            current_container++; data = ""; //osiągnięto koniec kontenera

            foreach (KeyValuePair<int, Output> output in links.output_ports)
            {

                if (output.Value.Port == port)
                {
                    foreach (SDHFrame frame in frames)
                    {
                        MD5 myHash = new MD5CryptoServiceProvider();
                        SDHFrame frame2 = frame;
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

                        output.Value.sendData(bytes_2.ToArray<byte>());
                        Thread.Sleep(50); // nie ruszać! bez tego ramki wysyłają się na raz i blokuje się socket
                    }
                }
            }
        }



    }
   
}
