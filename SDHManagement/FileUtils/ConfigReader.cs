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
       
        public static Returnable readPortsFromConfig (string configurationFileName)
        {
            List<int> portList = new List<int>();
            List<string> moduleList = new List<string>();
            List<string> contenerList = new List<string>();

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).FullName+"\\Configs\\Management"); //sobczakj 
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
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "module"))
                        {
                            if (configReader.HasAttributes)
                            {
                                moduleList.Add(configReader.GetAttribute("value"));
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "contener"))
                        {
                            if (configReader.HasAttributes)
                            {
                                contenerList.Add(configReader.GetAttribute("name"));
                            }
                        }

                    }
                    Returnable returnable = new Returnable(portList, moduleList, contenerList);
                    return returnable;

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }
        public static Dictionary<int,string> readCommandsFromConfig(string configurationFileName)
        {
            Dictionary<int, string> portCommandDictionary = new Dictionary<int, string>();

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).FullName + "\\Configs");
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
                                portCommandDictionary.Add(int.Parse(configReader.GetAttribute("value")), configReader.GetAttribute("command"));
                            }
                        }
                        
                    }
                    return portCommandDictionary;

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }

    }
}
