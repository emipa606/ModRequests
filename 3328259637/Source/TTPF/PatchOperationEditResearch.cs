using System;
using System.Collections;
using System.Linq;
using System.Xml;
using Verse;
using TTPF;
using System.Collections.Generic;
using RimWorld;

namespace TTPF
{

    /*
    * A custom patch operation to simplify sequence patch operations when defensively adding fields
    * Code by Lanilor (https://github.com/Lanilor)
    * This code is provided "as-is" without any warrenty whatsoever. Use it on your own risk.
    */
    public class PatchOperationEditResearch : PatchOperationPathed
    {
        public string doesRequire;
        public XmlContainer researchViewX;
        public XmlContainer researchViewY;
        public XmlContainer tab;
        public XmlContainer prerequisites;
        public XmlContainer hiddenPrerequisites;
        public XmlContainer baseCost;
        public XmlContainer techLevel;

        protected override bool ApplyWorker(XmlDocument xml)
        {

            if (!string.IsNullOrWhiteSpace(doesRequire))
            {
                string doesRequire = this.doesRequire;
                char[] chArray = new char[1] { ',' };
                foreach (string str in doesRequire.Split(chArray))
                {
                    if (!ModsConfig.IsActive(str.Trim()))
                        return true;
                }
            }

            bool result = false;

            foreach (XmlNode parentNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray<XmlNode>())
            {
                result = true;

                ReplaceNode(parentNode, "researchViewX", researchViewX);
                ReplaceNode(parentNode, "researchViewY", researchViewY);
                ReplaceNode(parentNode, "tab", tab);
                ReplaceNode(parentNode, "baseCost", baseCost);
                ReplaceNode(parentNode, "techLevel", techLevel);

                ReplaceChildren(parentNode, "prerequisites", prerequisites);
                ReplaceChildren(parentNode, "hiddenPrerequisites", hiddenPrerequisites);

            }

            return result;
        }

        private void ReplaceChildren(XmlNode parentNode, string nodeName, XmlContainer childrenContainer, bool keepOld = true)
        {
            if(childrenContainer == null) return;

            XmlNode node = parentNode.SelectSingleNode(nodeName);
            XmlNode childrenContainerNode = childrenContainer.node;

            if (node != null)
            {
                while (!keepOld || node.HasChildNodes)
                {
                    node.RemoveChild(node.FirstChild);
                }
                foreach (XmlNode children in childrenContainerNode.ChildNodes)
                {
                    XmlNode newchild = node.OwnerDocument.CreateNode(XmlNodeType.Element, "li", "");
                    newchild.InnerText = children.InnerText;
                    node.AppendChild(newchild);
                }
            }
        }

        private void ReplaceNode(XmlNode parentNode, string nodeName, XmlContainer valueContainer)
        {
            if(valueContainer == null) return;

            string value = valueContainer?.node.InnerText;

            if (string.IsNullOrWhiteSpace(value)) return;

            XmlNode node = parentNode.SelectSingleNode(nodeName);
            if (node != null)
            {
                node.InnerText = value;
            }
            else
            {
                XmlNode newnode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, "");
                newnode.InnerText = value;
                parentNode.AppendChild(newnode);
            }            
        }

    }



}