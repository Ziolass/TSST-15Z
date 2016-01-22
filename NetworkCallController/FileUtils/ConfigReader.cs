using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


namespace NetworkCallController.FileUtils
{
    class ConfigReader
    {
        private static string configurationFilePath;

        private static XmlReader configReader;

        private static string setupDirectoryPath(string path, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(path);
            sb.Append("\\");
            sb.Append(file);
            return sb.ToString();
        }

        public static Tuple<string, int,string,int,int,int> readConfig(string configurationFileName)
        {
            string network_identifier = "AS";
            int port = 0;
            int foreignPort = 0;
            string foreiggnNetworkIdentifier = "AS";
            int directoryPort=0;
            int policyPort=0;

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).FullName + "\\Configs\\NCC"); //sobczakj 
            configurationFilePath = setupDirectoryPath(di.ToString(), configurationFileName);
            using (
            configReader = XmlReader.Create(configurationFilePath))
                try
                {
                    while (configReader.Read())
                    {
                        if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "autonomous-system-name"))
                        {
                            if (configReader.HasAttributes)
                            {
                                network_identifier = configReader.GetAttribute("name");
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "localport"))
                        {
                            if (configReader.HasAttributes)
                            {
                                port = int.Parse(configReader.GetAttribute("value"));
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "other-ncc"))
                        {
                            if (configReader.HasAttributes)
                            {
                                foreiggnNetworkIdentifier = configReader.GetAttribute("name");
                                foreignPort = int.Parse(configReader.GetAttribute("value"));
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "dictionary"))
                        {
                            if (configReader.HasAttributes)
                            {
                                directoryPort= int.Parse(configReader.GetAttribute("value"));
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "policy"))
                        {
                            if (configReader.HasAttributes)
                            {
                                policyPort = int.Parse(configReader.GetAttribute("value"));
                            }
                        }

                    }
                    return Tuple.Create(network_identifier, port,foreiggnNetworkIdentifier,foreignPort,directoryPort,policyPort);

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }


    }
}