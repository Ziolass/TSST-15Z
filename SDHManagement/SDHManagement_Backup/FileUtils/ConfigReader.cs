using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SDHManagement2.FileUtils
{
    class ConfigReader
    {
        private static string configurationFilePath;

        private static XmlReader configReader;
        private static XmlWriter configUpdater;

        private static string setupDirectoryPath(string path, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(path);
            sb.Append("\\");
            sb.Append(file);
            return sb.ToString();
        }

        public static void addNewElement(string name, int port)
        {
            string configurationFileName = "\\managementConfigFile.xml";
            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            configurationFilePath = setupDirectoryPath(defaultDirectoryPath, configurationFileName);

            var dane = XElement.Load(configurationFilePath);
            var new_router_info = new XElement("router-port",
                 "name=\""+name+"\"",
                "port=\""+ port.ToString()+"\"");

            dane.Element("router-ports").Add(new_router_info);
            dane.Save(configurationFilePath);
        }
       
        public static List<int> readPortsFromConfig (string configurationFileName)
        {
            List<int> portList = new List<int>();

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo(((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).Parent).FullName+"\\Configs");
            configurationFilePath = setupDirectoryPath(di.ToString(), configurationFileName);
            using (
            configReader = XmlReader.Create(configurationFilePath))
                try
                {
                    while (configReader.Read())
                    {
                        if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "port"))
                        {
                            if (configReader.HasAttributes)
                            {
                                portList.Add(int.Parse(configReader.GetAttribute("value")));
                            }
                        }

                    }
                    return portList;

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }


    }
}
