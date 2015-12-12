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

namespace Client
{
    public partial class SDHClient : Form
    {
        private static int clientNumber = 0;
        private string nazwa = "";
        private String configurationFilePath;
        private List<WireCloud.Link> links = new List<WireCloud.Link>();
        //private List<IAsyncResult> iar = new List<IAsyncResult>();
        //  private Dictionary<WireCloud.Link, Thread> linksThreads;
        // private List<int> usedPorts;
        //  public List<WireCloud.Link> Links { get; private set; }
        private WireCloud.CloudSetupProcess cloud;
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
        public SDHClient()
        {
            InitializeComponent();
            clientNumber++;
            this.textBox2.Text += clientNumber.ToString();
            this.cloud = new WireCloud.CloudSetupProcess(configurationFilePath);
            this.Show();
            foreach (var link in cloud.Links) { 
            this.links.Add(link);
                this.links[links.Count - 1].StartListening();
        }
        }


    
        
        
        private void form_Resize(object sender, EventArgs e)
        {
            textBox1.Width = this.Width;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           var occupied =  cloud.TryAddLink((int)numericUpDown1.Value, (int)numericUpDown2.Value);
            DispConnections();
        }
        private void DispConnections()
        {
            listView1.Items.Clear();
            foreach (var item in cloud.Links)
            {
                string itm_to_str = item.Source + " -> " + item.Destination + "włączony:" + item.IsLinkActive;
               
                if (item.IsLinkActive)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { itm_to_str }, "", Color.Black, Color.LightGreen, DefaultFont));

                }
                else
                {
                    listView1.Items.Add(new ListViewItem(new string[] { itm_to_str }, "", Color.Black, Color.LightYellow, DefaultFont));


                }
                links.Add(cloud.Links[cloud.Links.Count - 1]);
                this.links[links.Count - 1].StartListening();

            }

        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter&&listView1.SelectedIndices.Count != 0)
            {
               // links[listView1.SelectedIndices[0]].sendData,)

            }
            else
            {
                MessageBox.Show("Nie zaznaczono łącza");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.Text = textBox2.Text;

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try {
                int index = listView1.SelectedIndices[0];
               // string txt = listView1.Items[index].Text;
              //  int pos = txt.IndexOf(' ');
                //int port_no = Int32.Parse(txt.Substring(0, pos));
                links[index].IsLinkActive = !links[index].IsLinkActive;
                if(links[index].IsLinkActive==true) this.links[index].StartListening();

            }
            catch(Exception) {}
            this.DispConnections();


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
  /*  public class ClientConnection
    {
        int p_in, p_out;
        public int port_in
        {
            get
            {
                return p_in;
            }
            set
            {
                p_in = value;
            }
        }
        public ClientConnection() { }


    }*/
}
