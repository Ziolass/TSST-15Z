using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using SDHClient;

using System.Windows.Forms;

namespace SDHClient
{
    class Program
    {

            static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(path).Parent).Parent).Parent).FullName+"\\Configs\\Client"); //sobczakj
            bool any_found = false;
            if(di.Exists)
            foreach (FileInfo fi in di.GetFiles("Klient*.xml"))
            {

                SDHClient sd = new SDHClient(fi.FullName);
                sd.Show();
                any_found = true;
                
                 
            }
            if (!any_found) { Console.WriteLine("KLIENT: Nie znaleziono żadnego pliku konf. klienta");  }

            Application.Run();
            
        }
    }
}
