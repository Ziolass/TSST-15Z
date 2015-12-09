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
        public static Dictionary<string,int> readConfig(string configurationFileName)
        {
            Dictionary<string,int> portDictionary = new Dictionary<string, int>();

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            configurationFilePath = setupDirectoryPath(defaultDirectoryPath, configurationFileName);
            using (
            configReader = XmlReader.Create(configurationFilePath))
                try
                {
                    while (configReader.Read())
                    {
                        if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "router-port")) 
                        {
                            if (configReader.HasAttributes)
                            {
                                portDictionary.Add(configReader.GetAttribute("name"),int.Parse(configReader.GetAttribute("port")));
                            }
                        }

                    }
                    return portDictionary;

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }



    }
}
