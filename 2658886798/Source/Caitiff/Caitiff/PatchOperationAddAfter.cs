using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Verse;

namespace Vampire
{
	public class PatchOperationAddAfter : PatchOperation
	{
		//public string defNode; // Get from patch command
		//public List<string> disciplines = new List<string>();

		#pragma warning disable 649
		//private string modName;
		private string disciplineName;
		private string referenceName;
		private string xpath;
		#pragma warning restore 649

		protected override bool ApplyWorker(XmlDocument xml)
		{
			xml.SelectSingleNode("Def/Vampire.BloodlineDef/ROMV_ClanCaitiff/disciplines").InsertAfter(xml.CreateElement("<li>ROMV_Dementation</li>"), xml.SelectSingleNode("<li>ROMV_BloodDiscipline</li>"));
			return true;
			//xpath = xml.GetXPath();
			//XmlNode disciplineNode = xml.SelectSingleNode(xpath);
			//XmlNode referenceNode = disciplineNode.SelectSingleNode(referenceName);
			//if (!disciplineName.NullOrEmpty() && !referenceName.NullOrEmpty())
			//{
			//	XmlNode xmlNode = xml.SelectSingleNode(xpath);
			//	XmlNode xmlChild = xml.SelectSingleNode(xpath).SelectSingleNode(referenceName);
			//	xmlNode.InsertAfter(xml.CreateElement(disciplineName), xmlChild);
			//	return true;
			//}
			//else return false;
		}

		//private string xpath;
		//private string defName;
		//private List<string> orderedStrings = new List<string>();
		//private string animalism = "<li>ROMV_Animalism</li>";
		//private string auspex = "<li>ROMV_Auspex</li>";
		//private string blood = "<li>ROMV_BloodDiscipline</li>";
		//private string dementation = "<li>ROMV_Dementation</li>";
		//private string dominate = "<li>ROMV_Dominate</li>";
		//private string flight = "<li>ROMV_Flight</li>";
		//private string fortitude = "<li>ROMV_Fortitude</li>";
		//private string obfuscation = "<li>ROMV_Obfuscation</li>";
		//private string obtenebration = "<li>ROMV_Obtenebration</li>";
		//private string potence = "<li>ROMV_Potence</li>";
		//private string presence = "<li>ROMV_Presence</li>";
		//private string protean = "<li>ROMV_Protean</li>";
		//private string thaumaturgy = "<li>ROMV_Thaumaturgy</li>";
		//private string vicissitude = "<li>ROMV_Vicissitude</li>";
		//
		//
		//public List<XmlNode> orderedNodes = new List<XmlNode>();
		//
		//public void AddDisciplinesToList(List<string> sDisciplines, out List<XmlNode> nDisciplines)
		//{
		//	sDisciplines.Add(animalism);
		//	sDisciplines.Add(auspex);
		//	sDisciplines.Add(blood);
		//	sDisciplines.Add(dementation);
		//	sDisciplines.Add(dominate);
		//	sDisciplines.Add(flight);
		//	sDisciplines.Add(fortitude);
		//	sDisciplines.Add(obfuscation);
		//	sDisciplines.Add(obtenebration);
		//	sDisciplines.Add(potence);
		//	sDisciplines.Add(presence);
		//	sDisciplines.Add(protean);
		//	sDisciplines.Add(thaumaturgy);
		//	sDisciplines.Add(vicissitude);
		//	nDisciplines = sDisciplines.Cast<XmlNode>().ToList<XmlNode>();
		//}
		//
		////protected List<XmlNode> nodes;
		//
		//
		//protected override bool ApplyWorker(XmlDocument xml)
		//{
		//	AddDisciplinesToList(orderedStrings, out orderedNodes);
		//	xml.SelectSingleNode(xpath).RemoveAll();
		//	while (true)
		//	{
		//		while (orderedNodes.Count() > 0)
		//		{
		//			xml.SelectSingleNode(xpath).AppendChild(orderedNodes[0]);
		//			orderedNodes.RemoveAt(0);
		//		}
		//		orderedNodes = null;
		//	}
		//}
		//	

		//foreach (XmlNode xmlNode in xml.SelectNodes(xpath)) // Fire for every node at XPath...
		//{
		//	List<XmlNode> list = xmlNode.SelectNodes(xpath/*DirectXmlSaveLoadUtility.GetXPath(xmlNode)*/ + "/li").Cast<XmlNode>().ToList(); // Create a list with 'li'
		//bool flag = reverse;
		//if (flag)
		//{
		//	bool flag2 = this.nonNumeric;
		//	if (flag2)
		//	{
		//		list.Sort(delegate (XmlNode node1, XmlNode node2)
		//		{
		//			XmlNode xmlNode2 = node1.SelectSingleNode(this.xpathLocal);
		//			XmlNode xmlNode3 = node2.SelectSingleNode(this.xpathLocal);
		//			bool flag4 = xmlNode2 == null && xmlNode3 == null;
		//			int result;
		//			if (flag4)
		//			{
		//				result = 0;
		//			}
		//			else
		//			{
		//				bool flag5 = xmlNode2 == null;
		//				if (flag5)
		//				{
		//					result = -1;
		//				}
		//				else
		//				{
		//					bool flag6 = xmlNode3 == null;
		//					if (flag6)
		//					{
		//						result = 1;
		//					}
		//					else
		//					{
		//						string innerText = xmlNode2.InnerText;
		//						string innerText2 = xmlNode3.InnerText;
		//						result = innerText2.CompareTo(innerText);
		//					}
		//				}
		//			}
		//			return result;
		//		});
		//	}
		//list.Sort(delegate (XmlNode node1, XmlNode node2)
		//{
		//	XmlNode xmlNode2 = node1.SelectSingleNode(xml.GetXPath());
		//	XmlNode xmlNode3 = node2.SelectSingleNode(xml.GetXPath());
		//	bool flag4 = xmlNode2 == null && xmlNode3 == null;
		//	int result;
		//	if (flag4)
		//	{
		//		result = 0;
		//	}
		//	else
		//	{
		//		bool flag5 = xmlNode2 == null;
		//		if (flag5)
		//		{
		//			result = -1;
		//		}
		//		else
		//		{
		//			bool flag6 = xmlNode3 == null;
		//			if (flag6)
		//			{
		//				result = 1;
		//			}
		//			else
		//			{
		//				float num = float.Parse(xmlNode2.InnerText);
		//				float num2 = float.Parse(xmlNode3.InnerText);
		//				result = ((num > num2) ? -1 : ((num < num2) ? 1 : 0));
		//			}
		//		}
		//	}
		//	return result;
		//});
		//
		//foreach (object obj in xmlNode.ChildNodes)
		//{
		//	XmlNode oldChild = (XmlNode)obj;
		//	xmlNode.RemoveChild(oldChild);
		//}
		//foreach (XmlNode newChild in list)
		//{
		//	xmlNode.AppendChild(newChild);
		//}
		//}
		//return true;
	}
}

//		private bool GetDisciplines(XmlDocument xml, XmlNode xmlNode, out List<XmlElement> disciplines)
//		{
//			var comps_nodes = xmlNode.ChildNodes;
//			List<XmlElement> comps_list = new List<XmlElement>();
//			int count = 0;
//
//			if (comps_nodes.Count == 0)
//			{
//				//output = xml.CreateElement(name);
//				//xmlNode.AppendChild(output);
//				disciplines = null;
//				return false;
//			}
//			else
//			{
//				while (count < comps_nodes.Count)
//				{
//					comps_list.Add(comps_nodes[count] as XmlElement);
//					count++;
//				}
//				disciplines = comps_list;
//				//output = comps_node. as XmlElement;
//				return true;
//			}
//		}
//	}
//}

		////foreach (var current in xml.SelectNodes("Defs/ThingDef[defName=\"" + defName + "\"]"))
		//foreach (var disciplines in xml.SelectNodes("Defs/Vampire.BloodlineDef[defName=\"" + defName + "\"]/disciplines"))
		//{
		//	result = true;
		//
		//	var xmlNode = disciplines as XmlNode;
		//
		//	// If we have a mod and a discipline, do stuff...
		//	if (!modName.NullOrEmpty() && !modDiscipline.NullOrEmpty() && ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName))
		//	{
		//		List<XmlElement> disciplineList = new List<XmlElement>();
		//		disciplineList.GetElementsByTagName;
		//	}
		//
		//
		//	if (statBases?.node.HasChildNodes ?? false)
		//	{
		//		AddOrReplaceStatBases(xml, xmlNode);
		//	}
		//	if (costList?.node.HasChildNodes ?? false)
		//	{
		//		AddOrReplaceCostList(xml, xmlNode);
		//	}
		//
		//	if (Properties != null && Properties.node.HasChildNodes)
		//	{
		//		AddOrReplaceVerbPropertiesCE(xml, xmlNode);
		//	}
		//
		//	if (AmmoUser != null || FireModes != null)
		//	{
		//		AddOrReplaceCompsCE(xml, xmlNode);
		//	}
		//
		//	if (weaponTags != null && weaponTags.node.HasChildNodes)
		//	{
		//		AddOrReplaceWeaponTags(xml, xmlNode);
		//	}
		//
		//	if (researchPrerequisite != null)
		//	{
		//		AddOrReplaceResearchPrereq(xml, xmlNode);
		//	}
		//
		//	// RunAndGun compatibility
		//	if (ModLister.HasActiveModWithName("RunAndGun") && !AllowWithRunAndGun)
		//	{
		//		AddRunAndGunExtension(xml, xmlNode);
		//	}
		//}

		//return result;
		//}


		//private void AddOrReplaceCostList(XmlDocument xml, XmlNode xmlNode)
		//{
		//	XmlElement costListElement;
		//	GetOrCreateNode(xml, xmlNode, "costList", out costListElement);
		//
		//	// Clear list first
		//	if (costListElement.HasChildNodes)
		//	{
		//		costListElement.RemoveAll();
		//	}
		//
		//	Populate(xml, costList.node, ref costListElement);
		//}
		//
		//private bool GetClanNode(XmlDocument all_clans, XmlNode clan, string name)
		//{
		//	var clan_node = clan.SelectNodes(name);
		//	List<XmlElement> clan_list = new List<XmlElement>();
		//	if (clan_node.Count != 0 && clan_node != null)
		//	{
		//		clan_list = clan.SelectNodes("Defs/Vampire.BloodlineDef[defName="ROMV_ClanCaitiff"]/disciplines");
		//
		//		output = xml.CreateElement(name);
		//		xmlNode.AppendChild(output);
		//		return false;
		//	}
		//	else
		//	{
		//		output = comps_nodes[0] as XmlElement;
		//		return true;
		//	}
		//}
		//#pragma warning disable 649
		//private string modName;
		//#pragma warning restore 649
		//private List<XmlElement> Alph(XmlDocument xml, XmlNode node)
		//{
		//	node.SelectNodes(node.ToString());
		//	
		//}
		//
		//protected override bool ApplyWorker(XmlDocument xml)
        //{
        //    return !modName.NullOrEmpty() && ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName);
        //}