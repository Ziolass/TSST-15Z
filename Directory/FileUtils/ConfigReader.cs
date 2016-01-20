using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


namespace Directory.FileUtils
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

        public static Tuple<string,Dictionary<string,int>> readEntriesFromConfig(string configurationFileName)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string network_identifier="AS";

            string defaultDirectoryPath = System.IO.Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo((((new DirectoryInfo(defaultDirectoryPath).Parent).Parent).Parent).FullName + "\\Configs\\Directory"); //sobczakj 
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
                        else if ((configReader.NodeType == XmlNodeType.Element) && (configReader.Name == "entry"))
                        {
                            if (configReader.HasAttributes)
                            {
                                dict.Add(configReader.GetAttribute("username"),int.Parse(configReader.GetAttribute("address")));
                            }
                        }
                        

                    }
                    return Tuple.Create(network_identifier, dict);

                }
                catch
                    (Exception e)
                {
                    return null;
                }
        }
        

    }
}
