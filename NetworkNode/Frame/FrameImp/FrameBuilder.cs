using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkNode.SDHFrame
{
    public class FrameBuilder : IFrameBuilder
    {
        private static JObject metadata;

        /// <summary>
        /// Builds the frame from JSON text
        /// </summary>
        /// <param name="textFrame">The text of JSON frame.</param>
        /// <returns></returns>
        public IFrame BuildFrame(String textFrame)
        {
            metadata = JObject.Parse(textFrame);
            StmLevel frameLevel = StmLevel.STM1;
            if (metadata["Level"] != null)
            {
                frameLevel = FrameBuilder.getStmLevel(metadata["Level"]);
            }
            Frame returnFrame = new Frame(frameLevel);
            if (metadata["Msoh"].HasValues)
                returnFrame.Msoh = (Header)FrameBuilder.EvaluateContent((JObject)metadata["Msoh"]);
            if (metadata["Rsoh"].HasValues)
                returnFrame.Rsoh = (Header)FrameBuilder.EvaluateContent((JObject)metadata["Rsoh"]);            
            if (FrameBuilder.isJArray(metadata["Content"]))
            {
                returnFrame.Content = FrameBuilder.evaluateContents((JArray)metadata["Content"]);
            }
            else return null;
            return returnFrame;
        }
        /// <summary>
        /// Builds the frame from JSON file.
        /// </summary>
        /// <param name="pathToJson">The path to JSON file.</param>
        /// <returns></returns>
        public IFrame BuildFrameFromFile(string pathToJson)
        {
            try
            {
                return BuildFrame(File.ReadAllText(pathToJson));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Builds the literal.
        /// </summary>
        /// <param name="textFrame">The frame object.</param>
        /// <returns></returns>
        public String BuildLiteral(IFrame textFrame)
        {
            return JsonConvert.SerializeObject(textFrame);
        }

        /// <summary>
        /// Evaluates the content. Read JSON object create IContent
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private static IContent EvaluateContent(JObject content)
        {
            try
            {
                if (FrameBuilder.isVirtualContainer(content["Type"])) //VirtualContainer
                {
                    //Create new VC with level from JSON file
                    VirtualContainer newVC = new VirtualContainer(FrameBuilder.getVCLevel(content["Level"]));
                    newVC.Pointer = content["Pointer"].ToString();
                    newVC.POH = (POH)FrameBuilder.EvaluateContent((JObject)content["POH"]);
                    if (FrameBuilder.isJArray(content["Content"]))
                    {
                        newVC.Content = FrameBuilder.evaluateContents((JArray)content["Content"]);
                    }
                    else //There is no value Content of VC is null
                    {
                        newVC.Content = null;
                    }
                    return newVC;
                }
                else if (FrameBuilder.isContainer(content["Type"]))
                {
                    Container newContainer = new Container(content["Content"].ToString());
                    return newContainer;
                }
                else if (FrameBuilder.isHeader(content["Type"]))
                {
                    string checksum = content["Checksum"].ToString();
                    string eow = content["EOW"].ToString();
                    string dcc = content["DCC"].ToString();
                    Header newHeader = new Header(checksum, eow, dcc);
                    return newHeader;
                }
                else if (FrameBuilder.isPOH(content["Type"]))
                {
                    SignalLabelType signalType = FrameBuilder.getSignalType(content["SignalLabel"]);
                    POH poh = new POH(signalType);
                    return poh;
                }
                else return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        /// <summary>
        /// Evaluates the contents. Iterate through the JArray to create IContent
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
                    returnList.Add(FrameBuilder.EvaluateContent((JObject)item));
                }
                else //Index in Frame.Content is free or occupied by larger VC then VC12
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
        /// Determines whether the specified token is VirtualContainer.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isVirtualContainer(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.VICONTAINER)
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified JSON token is Container.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isContainer(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.CONTAINER)
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified token is header.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isHeader(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.HEADER)
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Determines whether the specified token is POH.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool isPOH(JToken token)
        {
            try
            {
                if (getContentType(token) == ContentType.POH)
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">ERROR FrameBuilder: Could not read content type of given token</exception>
        private static ContentType getContentType(JToken token)
        {
            ContentType contentType;
            if (Enum.TryParse<ContentType>(token.ToString(), out contentType))
                return contentType;
            else
                throw new Exception("ERROR FrameBuilder: Could not read content type of given token");
        }

        /// <summary>
        /// Gets the vc level.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">ERROR FrameBuilder: Could not read level of given token</exception>
        private static VirtualContainerLevel getVCLevel(JToken token)
        {
            VirtualContainerLevel containerLevel;
            if (Enum.TryParse<VirtualContainerLevel>(token.ToString(), out containerLevel))
                return containerLevel;
            else
                throw new Exception("ERROR FrameBuilder: Could not read level of given token");
        }

        /// <summary>
        /// Gets the tu level.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">ERROR FrameBuilder: Could not read level of given token</exception>
        private static VirtualContainerLevel getTULevel(JToken token)
        {
            VirtualContainerLevel containerLevel;
            if (Enum.TryParse<VirtualContainerLevel>(token.ToString(), out containerLevel))
                return containerLevel;
            else
                throw new Exception("ERROR FrameBuilder: Could not read level of given token");
        }
        /// <summary>
        /// Gets the type of the signal.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">ERROR FrameBuilder: Could not read content type of given token</exception>
        private static SignalLabelType getSignalType(JToken token)
        {
            SignalLabelType contentType;
            if (Enum.TryParse<SignalLabelType>(token.ToString(), out contentType))
                return contentType;
            else
                throw new Exception("ERROR FrameBuilder: Could not read content type of given token");
        }
        private static StmLevel getStmLevel(JToken token)
        {
            StmLevel contentType;
            if (Enum.TryParse<StmLevel>(token.ToString(), out contentType))
                return contentType;
            else
                throw new Exception("ERROR FrameBuilder: Could not read STM level of given token");
        }
    }
}
