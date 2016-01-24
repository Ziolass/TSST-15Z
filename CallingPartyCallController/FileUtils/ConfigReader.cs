
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CallingPartyCallController.FileUtils
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

        public static Tuple<string, int,int> readConfig(string configurationFileName)
        {
            string network_identifier = "AS";
            int localport = 0;
            int nccPort = 0;

            string defaultDirectoryPath = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).FullName + "\\Configs\\CPCC"); //sobczakj 
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
                                localport = int.Parse(configReader.GetAttribute("value"));
                            }
                        }
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "ncc-port"))
                        {
                            if (configReader.HasAttributes)
                            {
                                nccPort = int.Parse(configReader.GetAttribute("value"));
                            }
                        }


                    }
                    return Tuple.Create(network_identifier, localport, nccPort);

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }


    }
}

