<<<<<<< HEAD
﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkNode.Frame
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
            SDHFrame returnFrame = new SDHFrame();
            metadata = JObject.Parse(textFrame);
            if (FrameBuilder.isHeader(metadata["Msoh"]))
            {
                returnFrame.Msoh = (Header)FrameBuilder.evaluateContent((JObject)metadata["Msoh"]);

            }
            if (FrameBuilder.isHeader(metadata["Rsoh"]))
            {
                returnFrame.Msoh = (Header)FrameBuilder.evaluateContent((JObject)metadata["Rsoh"]);

            }
            if (FrameBuilder.isJArray(metadata["Content"]))
            {
                returnFrame.Content = FrameBuilder.evaluateContents((JArray)metadata["Content"]);
            }
            else return null;
            return returnFrame;
        }

        /// <summary>
        /// Builds the empty frame.
        /// </summary>
        /// <returns></returns>
        public IFrame BuildEmptyFrame()
        {
            return new SDHFrame();
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
        private static IContent evaluateContent(JObject content)
        {
            try
            {
                if (FrameBuilder.isVC(content["Type"])) //VirtualContainer
                {
                    //Create new VC with level from JSON file
                    VirtualContainer newVC = new VirtualContainer(FrameBuilder.getVCLevel(content["Level"]));
                    newVC.Pointer = content["Pointer"].ToString();
                    newVC.POH = content["POH"].ToString();
                    if (FrameBuilder.isObjectOfContainer(content["Content"])) //Check if "Content" has value and is type of Container
                    {
                        Container newContainer = (Container)FrameBuilder.evaluateContent((JObject)content["Content"]);
                        if (newContainer != null)
                        {
                            newVC.Content = newContainer;
                            return newVC;
                        }
                        else return null;
                    }
                    else //There is no value Content of VC is null
                    {
                        newVC.Content = null;
                        return newVC;
                    }
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
                    Header newHeader = new Header(checksum, eow);
                    return newHeader;
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
        /// Determines whether the specified jToken is object of container.
        /// </summary>
        /// <param name="jToken">The jToken.</param>
        /// <returns></returns>
        private static bool isObjectOfContainer(JToken jToken)
        {
            if (!jToken.HasValues || !FrameBuilder.isContainer(jToken["Type"]))
                return false;
            else
                return true;
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
                    returnList.Add(FrameBuilder.evaluateContent((JObject)item));
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
        private static bool isVC(JToken token)
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
        private static ContainerLevel getTULevel(JToken token)
        {
            ContainerLevel containerLevel;
            if (Enum.TryParse<ContainerLevel>(token.ToString(), out containerLevel))
                return containerLevel;
            else
                throw new Exception("ERROR FrameBuilder: Could not read level of given token");
        }
    }
=======
﻿using Newtonsoft.Json;
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
            Frame returnFrame = new Frame();
            metadata = JObject.Parse(textFrame);
            if (FrameBuilder.isHeader(metadata["Msoh"]))
            {
                returnFrame.Msoh = (Header)FrameBuilder.evaluateContent((JObject)metadata["Msoh"]);
            }
            if (FrameBuilder.isHeader(metadata["Rsoh"]))
            {
                returnFrame.Msoh = (Header)FrameBuilder.evaluateContent((JObject)metadata["Rsoh"]);
            }
            if (FrameBuilder.isJArray(metadata["Content"]))
            {
                returnFrame.Content = FrameBuilder.evaluateContents((JArray)metadata["Content"]);
            }
            else return null;
            return returnFrame;
        }

        /// <summary>
        /// Builds the empty frame.
        /// </summary>
        /// <returns></returns>
        public IFrame BuildEmptyFrame()
        {
            return new Frame();
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
        private static IContent evaluateContent(JObject content)
        {
            try
            {
                if (FrameBuilder.isVC(content["Type"])) //VirtualContainer
                {
                    //Create new VC with level from JSON file
                    VirtualContainer newVC = new VirtualContainer(FrameBuilder.getVCLevel(content["Level"]));
                    newVC.Pointer = content["Pointer"].ToString();
                    newVC.POH = content["POH"].ToString();
                    if (FrameBuilder.isObjectOfContainer(content["Content"])) //Check if "Content" has value and is type of Container
                    {
                        Container newContainer = (Container)FrameBuilder.evaluateContent((JObject)content["Content"]);
                        if (newContainer != null)
                        {
                            newVC.Content = newContainer;
                            return newVC;
                        }
                        else return null;
                    }
                    else //There is no value Content of VC is null
                    {
                        newVC.Content = null;
                        return newVC;
                    }
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
                else return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified jToken is object of container.
        /// </summary>
        /// <param name="jToken">The jToken.</param>
        /// <returns></returns>
        private static bool isObjectOfContainer(JToken jToken)
        {
            if (!jToken.HasValues || !FrameBuilder.isContainer(jToken["Type"]))
                return false;
            else
                return true;
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
                    returnList.Add(FrameBuilder.evaluateContent((JObject)item));
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
        private static bool isVC(JToken token)
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
    }
>>>>>>> dev
}