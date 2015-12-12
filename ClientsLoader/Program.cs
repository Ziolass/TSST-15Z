using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Client;

using System.Windows.Forms;

namespace ClientsLoader
{
    class Program
    {

            static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            bool any_found = false;

            foreach (FileInfo fi in di.GetFiles("Klient*.xml"))
            {

                Client.SDHClient sd = new Client.SDHClient(fi.FullName);
                sd.Show();
                any_found = true;
                
                 
            }
            Application.Run();
            if (!any_found) { MessageBox.Show("Klient: Nie znaleziono żadnego pliku konfiguracyjnego xml"); }
            
        }
    }
}
