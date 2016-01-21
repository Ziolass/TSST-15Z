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

        public static Tuple<string, int> readConfig(string configurationFileName)
        {
            string network_identifier = "AS";
            int port = 0;

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
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "port"))
                        {
                            if (configReader.HasAttributes)
                            {
                                port = int.Parse(configReader.GetAttribute("value"));
                            }
                        }
                        
                    }
                    return Tuple.Create(network_identifier, port);

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }


    }
}