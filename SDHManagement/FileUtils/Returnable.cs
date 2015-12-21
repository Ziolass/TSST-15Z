using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDHManagement2.FileUtils
{
    class Returnable
    {
        public List<int> portList
        {
            get; set;
        }
        public List<string> moduleList
        {
            get; set;
        }
        public List<string> contenerList { get; set; }


        public Returnable(List<int> ports, List<string> modules, List<string> conteners)
        {
            portList = ports;
            moduleList = modules;
            contenerList = conteners;
        }
        
    }
}
