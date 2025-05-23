﻿using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Verse;

namespace DualWield.Settings
{
    public class DictRecordHandler : SettingHandleConvertible
    {
        public Dictionary<String, Record> inner = new Dictionary<String, Record>();
        public Dictionary<String, Record> InnerList { get { return inner; } set { inner = value; } }
        private XmlSerializer serializer = new XmlSerializer(typeof(Record));

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<String, Record>();
            if (!settingValue.Equals(string.Empty))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = settingValue;
                Dictionary<String, Record> dict = new Dictionary<string, Record>();
                String name = xmlDoc.FirstChild.Name;
                foreach(XmlNode recordNode in xmlDoc.FirstChild.ChildNodes)
                {
                    StringReader rdr = new StringReader(recordNode.InnerXml);
                    dict.Add(XmlConvert.DecodeName(recordNode.Name), (Record)serializer.Deserialize(rdr));
                }
                inner = dict;

            }
        }

        public override string ToString()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.AppendChild(xmlDoc.CreateElement("DictRecordHandler"));
            foreach (KeyValuePair<String, Record> child in inner)
            {
                XmlElement childXml = null;
                try
                {
                    childXml = xmlDoc.CreateElement(XmlConvert.VerifyName(child.Key));
                }
                catch (XmlException e)
                {
                    childXml = xmlDoc.CreateElement(XmlConvert.EncodeName(child.Key));
                    //Log.Warning("Couldn't create Dual Wield settings for: " + child.Key + ", because this defname isn't suitable for XML storage");
                }
                childXml.InnerXml = SerializeToString(child.Value);
                root.AppendChild(childXml);
            }
            return xmlDoc.OuterXml;
        }
        // To Clean XML
        private string SerializeToString<T>(T value)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, value, emptyNamespaces);
                return stream.ToString();
            }
        }
    }

}
