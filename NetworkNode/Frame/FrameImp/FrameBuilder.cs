using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NetworkNode.Frame
{
    public class FrameBuilder : IFrameBuilder
    {
        private static JObject metadata;
        /// <summary>
        /// Builds the frame.
        /// </summary>
        /// <param name="textFrame">The text of JSON frame.</param>
        /// <returns></returns>
        public IFrame BuildFrame(String textFrame)
        {
            Frame returnFrame = new Frame();
            metadata = JObject.Parse(textFrame);
            returnFrame.Msoh = metadata["MSOH"].ToObject<string>();
            returnFrame.Rsoh = metadata["RSOH"].ToObject<string>();

            if (FrameBuilder.isJArray(metadata["CONTENT"]))
            {
                returnFrame.Content = FrameBuilder.evaluateContents((JArray)metadata["CONTENT"]);
                return returnFrame;
            }
            else 
            {
                return null;
            }
        }
        public String BuildLiteral(IFrame textFrame) {
            String JFrame = String.Empty;
            JFrame = JsonConvert.SerializeObject(textFrame);
            return JsonConvert.SerializeObject(textFrame);         
        }

        public IFrame BuildEmptyFrame()
        {
            return new Frame();
        }
        
        /// <summary>
        /// Builds the frame from file.
        /// </summary>
        /// <param name="pathToJson">The path to json.</param>
        /// <returns></returns>
        public IFrame BuildFrameFromFile(string pathToJson)
        {
            return BuildFrame(File.ReadAllText(pathToJson));
        }
        private static IContent evaluateContent(JObject content)
        {
            if (FrameBuilder.isVC(content["TYPE"])) //VirtualContainer
            {
                VirtualContainer newVC = new VirtualContainer(FrameBuilder.getVCLevel(content["LEVEL"]));
                newVC.Pointer = content["POINTER"].ToString();
                newVC.POH = content["POH"].ToString();
                if (FrameBuilder.isObjectOfContainer(content["CONTENT"]))
                {
                    Container newContainer = (Container)FrameBuilder.evaluateContent((JObject)content["CONTENT"]);
                    if (newContainer != null)
                    {
                        newVC.Content = newContainer;
                        return newVC;
                    }
                    else return null;
                }
                else
                {
                    newVC.Content = null;
                    return newVC;
                }
            }
            else if (FrameBuilder.isContainer(content["TYPE"]))
            {
                Container newContainer = new Container(content["CONTENT"].ToString());
                return newContainer;
            }
            else return null;
        }
        private static IContent evaluateContent(JArray content)
        {
            if (content.Count == 1)
            {
                return evaluateContent((JObject)content[0]);
            }
            else return null;
        }
        private static bool isJArrayOfVC(JToken jToken)
        {
            if (FrameBuilder.isJArray(jToken))
            {
                foreach (var item in ((JArray)jToken))
                {
                    if (!FrameBuilder.isVC(item["TYPE"]))
                        return false;
                }
                return true;
            }
            else return false;
        }
        private static bool isObjectOfContainer(JToken jToken)
        {
            if (!jToken.HasValues || !FrameBuilder.isContainer(jToken["TYPE"]))
                        return false;
                    else
                return true;
        }
        /// <summary>
        /// Evaluates the contents. Iterate through the JArray to find containers and data
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private static List<IContent> evaluateContents(JArray content)
        {
            List<IContent> returnList = new List<IContent>();
            foreach (var item in content)
            {
                if (item.HasValues)
                {

                    returnList.Add(FrameBuilder.evaluateContent((JObject)item));
                }
                else
                {
                    returnList.Add(null);
                }
            }
            return returnList;
        }
        /// <summary>
        /// Determines whether the specified token is JArray.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isJArray(JToken token)
        {
            if (JToken.Parse(token.ToString()) is JArray)
                return true;
            else return false;
        }
        /// <summary>
        /// Determines whether the specified token is container.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isVC(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.VICONTAINER)
                    return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }
        private static bool isTU(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.TRIBUTARYUNIT)
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// Determines whether the specified token is data.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isContainer(JToken token)
        {
            if (getContentType(token) == ContentType.CONTAINER)
                return true;
            else return false;
        }
        private static ContentType getContentType(JToken token)
        {
            ContentType contentType;
            if (Enum.TryParse<ContentType>(token.ToString(), out contentType))
                return contentType;
            else
                throw new Exception("ERROR FrameBuilder: Could not read content type of given token");
        }
        private static VirtualContainerLevel getVCLevel(JToken token)
        {
            VirtualContainerLevel containerLevel;
            if (Enum.TryParse<VirtualContainerLevel>(token.ToString(), out containerLevel))
                return containerLevel;
            else
                throw new Exception("ERROR FrameBuilder: Could not read level of given token");
        }
        private static ContainerLevel getTULevel(JToken token)
        {
            ContainerLevel containerLevel;
            if (Enum.TryParse<ContainerLevel>(token.ToString(), out containerLevel))
                return containerLevel;
            else
                throw new Exception("ERROR FrameBuilder: Could not read level of given token");
        }
    }
}
