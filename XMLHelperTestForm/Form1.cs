using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XmlHelper;
using System.Xml;
namespace XMLHelperTestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //string va = XMLHelper.instance.XMLFiles[textBox1.Text.Trim()].GetXmlAttributeValue(textBox2.Text.Trim());

            XmlNode node = XMLHelper.instance.XMLFiles["Comment"].XmlDoc.SelectSingleNode("Comments");
            Dictionary<string, Dictionary<string, string>> CommentDic = new Dictionary<string, Dictionary<string, string>>();
            foreach (XmlNode item in node.ChildNodes)
            {
                string nodeName = item.Name;
                string path = "Comments." + nodeName + ".[*].{*}";
                string va = XMLHelper.instance.XMLFiles["Comment"].GetXmlAttributeValue(path);

                //[E,Cash Handle,];[B,Power Failure,]
                string[] statusArray = va.Split(';');
                Dictionary<string, string> newDic = new Dictionary<string, string>();
                foreach (string statusStr in statusArray)
                {
                    string statusTemp = statusStr.Replace("[", "").Replace("]", "");
                    string[] tempArray = statusTemp.Split(',');
                    newDic.Add(tempArray[0], tempArray[1]);
                }
                CommentDic.Add(nodeName, newDic);

            }

            //MessageBox.Show(va);
        }
    }
}
