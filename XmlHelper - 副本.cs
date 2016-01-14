using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace XmlHelper
{
    public class XMLHelper
    {
        private XMLHelper()
        {
            XmlDocument objDoc = new XmlDocument();
            string path = System.Environment.CurrentDirectory + @"\XMLFileList.xml";
            objDoc.Load(path);
            XmlNodeList xmlFiles = objDoc.DocumentElement.SelectSingleNode("Files").ChildNodes;
            if (xmlFiles != null && xmlFiles.Count > 0)
            {
                foreach (XmlNode voiceItem in xmlFiles)
                {
                    if (voiceItem.NodeType != XmlNodeType.Element)
                        continue;
                    XmlAttribute keyAttr = voiceItem.Attributes["key"];
                    XmlAttribute pathAttr = voiceItem.Attributes["path"];
                    if (null != keyAttr && !string.IsNullOrEmpty(keyAttr.Value)
                        && null != pathAttr && !string.IsNullOrEmpty(pathAttr.Value))
                    {
                        string pathN = System.Environment.CurrentDirectory + (pathAttr.Value.StartsWith(@"\") ? pathAttr.Value : @"\" + pathAttr.Value);
                        XmlDocument XmldocN = new XmlDocument();
                        XmldocN.Load(pathN);
                        XmlDocumentEx xmlDocEx = new XmlDocumentEx();
                        xmlDocEx.XmlDoc = XmldocN;
                        XMLFiles.Add(keyAttr.Value, xmlDocEx);
                    }
                }
            }
        }
        private static XMLHelper _instance;
        public static XMLHelper instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new XMLHelper();
                }
                return _instance;
            }
        }
        /// <summary>
        /// FileName
        /// </summary>
        public Dictionary<string, XmlDocumentEx> XMLFiles = new Dictionary<string, XmlDocumentEx>();
    }
    /// <summary>
    /// 自定义XML类型
    /// </summary>
    public class XmlDocumentEx
    {
        private XmlDocument _xmlDoc;
        internal XmlDocument XmlDoc
        {
            get { return _xmlDoc; }
            set { _xmlDoc = value; }
        }

        /// <summary>
        /// 获取特定节点下的属性值或innerText.
        /// 应用形式：
        /// A.B.C.[D].{E}
        /// A.B.C.[D].{E;F;G}
        /// A.B.C.[*].{E;F;G}
        /// A.B.C.[*].{*}
        /// A.B.C.[*]等到value或者innerText
        /// </summary>
        /// <param name="attriPath"></param>
        /// <returns></returns>
        public string GetXmlAttributeValue(string attriPath)
        {
            try
            {
                if (string.IsNullOrEmpty(attriPath))
                    return null;

                string[] nodeArray = attriPath.Split('.');
                XmlNodeList currNodeList = null;
                XmlNode currNode = null;
                string attriValue = string.Empty;
                bool isContainAttri = false;
                bool isSpecifiedKey = false;
                bool isAllKeys = false;
                foreach (string node in nodeArray)
                {
                    //定位节点key
                    if (node.StartsWith("["))
                    {
                        string itemT = node.Replace("[", "").Replace("]", "");
                        if (itemT.Equals("*"))
                        {
                            isAllKeys = true;

                            currNodeList = currNode.ChildNodes;

                            currNode = null;
                        }
                        else
                        {
                            foreach (XmlNode item in currNode.ChildNodes)
                            {
                                XmlAttribute xmlattri = item.Attributes["key"];
                                if (xmlattri != null && xmlattri.Value.Equals(itemT))
                                {
                                    isSpecifiedKey = true;
                                    currNode = item;
                                    break;
                                }
                            }
                            if (!isSpecifiedKey)
                            {
                                currNode = null;
                                break;
                            }
                        }
                    }
                    else if (node.StartsWith("{"))
                    {
                        string nodeTs = node.Replace("{", "").Replace("}", "");
                        if (currNodeList != null && currNodeList.Count > 0)
                        {
                            string tempAttriValue = string.Empty;
                            foreach (XmlNode item in currNodeList)
                            {
                                tempAttriValue = string.Empty;
                                //所有属性*
                                if (nodeTs.Trim().Equals("*") && item.Attributes.Count > 0)
                                {
                                    foreach (XmlAttribute attri in item.Attributes)
                                    {
                                        //获取指定节点指定属性值并（多个则叠加）
                                        isContainAttri = true;
                                        tempAttriValue += attri.Value + ",";
                                    }
                                }
                                else
                                {
                                    string[] nodeTsArray = nodeTs.Split(',');
                                    foreach (string nodeTss in nodeTsArray)
                                    {
                                        if (item.Attributes[nodeTss] != null)
                                        {
                                            //获取指定节点指定属性值并（多个则叠加）
                                            isContainAttri = true;
                                            tempAttriValue += item.Attributes[nodeTss].Value + ",";
                                        }
                                    }
                                }
                                //去掉最后一个分号;
                                if (tempAttriValue.EndsWith(";"))
                                    tempAttriValue = tempAttriValue.Substring(0, tempAttriValue.Length - 1);

                                tempAttriValue = "[" + tempAttriValue + "]";
                                attriValue += tempAttriValue + ";";
                            }
                        }
                        else
                        {

                            //所有属性*
                            if (nodeTs.Trim().Equals("*") && currNode.Attributes.Count > 0)
                            {
                                foreach (XmlAttribute attri in currNode.Attributes)
                                {
                                    //获取指定节点指定属性值并（多个则叠加）
                                    isContainAttri = true;
                                    attriValue += attri.Value + ";";
                                }
                            }
                            else
                            {
                                string[] nodeTsArray = nodeTs.Split(';');
                                foreach (string item in nodeTsArray)
                                {
                                    if (currNode.Attributes[item] != null)
                                    {
                                        //获取指定节点指定属性值并（多个则叠加）
                                        isContainAttri = true;
                                        attriValue += currNode.Attributes[item].Value + ";";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //定位节点深度递归
                        if (currNode == null)
                            currNode = XmlDoc.SelectSingleNode(node);
                        else
                            currNode = currNode.SelectSingleNode(node);
                    }
                }
                //如果没有指定相应的属性，也就是以[___]结尾，则首先看有没有value属性，如果不存在value属性，则返回该节点的InnerText.
                if (!isContainAttri)
                {
                    if (currNodeList != null && currNodeList.Count > 0)
                    {
                        string tempAttriValue = string.Empty;
                        foreach (XmlNode xn in currNodeList)
                        {
                            tempAttriValue = string.Empty;
                            if (xn.Attributes["value"] != null)
                                tempAttriValue += xn.Attributes["value"].Value;
                            else
                            {
                                tempAttriValue += xn.InnerText;
                            }
                            attriValue += "[" + tempAttriValue + "]";
                        }
                    }
                    else
                    {
                        if (currNode != null)
                        {
                            if (currNode.Attributes["value"] != null)
                                attriValue = currNode.Attributes["value"].Value;
                            else
                            {
                                attriValue = currNode.InnerText;
                            }
                        }
                    }
                }
                //去掉最后一个分号;
                if (attriValue.EndsWith(";"))
                    attriValue = attriValue.Substring(0, attriValue.Length - 1);

                WriteLogs("get xml attribute successed.");
                return attriValue;

            }
            catch (Exception ex)
            {
                WriteLogs(ex.Message);
                return null;
            }
        }


        private void WriteLogs(string msg)
        {
            string path = System.Environment.CurrentDirectory + @"\xml_log.txt";
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(msg);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }
    }
}
