using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace Vampire
{
	// Token: 0x02000057 RID: 87
	internal class PatchOperationSort : PatchOperationExtendedPathed
	{
		// Token: 0x06000121 RID: 289 RVA: 0x0000F27C File Offset: 0x0000D47C
		protected override bool Patch(XmlDocument xml)
		{
			foreach (XmlNode xmlNode in this.nodes)
			{
				List<XmlNode> list = xmlNode.SelectNodes(DirectXmlSaveLoadUtility.GetXPath(xmlNode) + "/li").Cast<XmlNode>().ToList<XmlNode>();
				bool flag = this.reverse;
				if (flag)
				{
					bool flag2 = this.nonNumeric;
					if (flag2)
					{
						list.Sort(delegate (XmlNode node1, XmlNode node2)
						{
							XmlNode xmlNode2 = node1.SelectSingleNode(this.xpathLocal);
							XmlNode xmlNode3 = node2.SelectSingleNode(this.xpathLocal);
							bool flag4 = xmlNode2 == null && xmlNode3 == null;
							int result;
							if (flag4)
							{
								result = 0;
							}
							else
							{
								bool flag5 = xmlNode2 == null;
								if (flag5)
								{
									result = -1;
								}
								else
								{
									bool flag6 = xmlNode3 == null;
									if (flag6)
									{
										result = 1;
									}
									else
									{
										string innerText = xmlNode2.InnerText;
										string innerText2 = xmlNode3.InnerText;
										result = innerText2.CompareTo(innerText);
									}
								}
							}
							return result;
						});
					}
					else
					{
						list.Sort(delegate (XmlNode node1, XmlNode node2)
						{
							XmlNode xmlNode2 = node1.SelectSingleNode(this.xpathLocal);
							XmlNode xmlNode3 = node2.SelectSingleNode(this.xpathLocal);
							bool flag4 = xmlNode2 == null && xmlNode3 == null;
							int result;
							if (flag4)
							{
								result = 0;
							}
							else
							{
								bool flag5 = xmlNode2 == null;
								if (flag5)
								{
									result = -1;
								}
								else
								{
									bool flag6 = xmlNode3 == null;
									if (flag6)
									{
										result = 1;
									}
									else
									{
										float num = float.Parse(xmlNode2.InnerText);
										float num2 = float.Parse(xmlNode3.InnerText);
										result = ((num > num2) ? -1 : ((num < num2) ? 1 : 0));
									}
								}
							}
							return result;
						});
					}
				}
				else
				{
					bool flag3 = this.nonNumeric;
					if (flag3)
					{
						list.Sort(delegate (XmlNode node1, XmlNode node2)
						{
							XmlNode xmlNode2 = node1.SelectSingleNode(this.xpathLocal);
							XmlNode xmlNode3 = node2.SelectSingleNode(this.xpathLocal);
							bool flag4 = xmlNode2 == null && xmlNode3 == null;
							int result;
							if (flag4)
							{
								result = 0;
							}
							else
							{
								bool flag5 = xmlNode2 == null;
								if (flag5)
								{
									result = -1;
								}
								else
								{
									bool flag6 = xmlNode3 == null;
									if (flag6)
									{
										result = 1;
									}
									else
									{
										string innerText = xmlNode2.InnerText;
										string innerText2 = xmlNode3.InnerText;
										result = innerText.CompareTo(innerText2);
									}
								}
							}
							return result;
						});
					}
					else
					{
						list.Sort(delegate (XmlNode node1, XmlNode node2)
						{
							XmlNode xmlNode2 = node1.SelectSingleNode(this.xpathLocal);
							XmlNode xmlNode3 = node2.SelectSingleNode(this.xpathLocal);
							bool flag4 = xmlNode2 == null && xmlNode3 == null;
							int result;
							if (flag4)
							{
								result = 0;
							}
							else
							{
								bool flag5 = xmlNode2 == null;
								if (flag5)
								{
									result = -1;
								}
								else
								{
									bool flag6 = xmlNode3 == null;
									if (flag6)
									{
										result = 1;
									}
									else
									{
										float num = float.Parse(xmlNode2.InnerText);
										float num2 = float.Parse(xmlNode3.InnerText);
										result = ((num > num2) ? 1 : ((num < num2) ? -1 : 0));
									}
								}
							}
							return result;
						});
					}
				}
				foreach (object obj in xmlNode.ChildNodes)
				{
					XmlNode oldChild = (XmlNode)obj;
					xmlNode.RemoveChild(oldChild);
				}
				foreach (XmlNode newChild in list)
				{
					xmlNode.AppendChild(newChild);
				}
			}
			return true;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0000F43C File Offset: 0x0000D63C
		protected override void SetException()
		{
			base.CreateExceptions(new string[]
			{
				this.xpath,
				"xpath",
				this.xpathLocal,
				"xpathLocal"
			});
		}

		// Token: 0x040000CC RID: 204
		public string xpathLocal = "li";

		// Token: 0x040000CD RID: 205
		public bool reverse = false;

		// Token: 0x040000CE RID: 206
		public bool nonNumeric = false;
	}
}


namespace Vampire
{
	public class PatchOperationDisAlph : PatchOperation
	{
		public string defNode; // Get from patch command
		//public List<string> disciplines = new List<string>();

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode xmlNode = xml.SelectSingleNode(defNode);


			if (!defNode.NullOrEmpty())
			{
				// Get the discipline node as a list...
				GetDisciplines(xml, xmlNode, out List<XmlElement> discipline_list);

				// Order the discipline list...

				discipline_list.OrderBy(key => discipline_list[0]);

				// Empty the node...
				xmlNode.RemoveAll();

				// Add the disciplines back in alphabetical order, then remove that item from the list...
				while (discipline_list.Count > 0)
				{
					xmlNode.AppendChild(discipline_list[0]);
					discipline_list.RemoveAt(0);
				}
				discipline_list = null; // Get rid of the list
				return true;
			}

			else return false;
		}

		private bool GetDisciplines(XmlDocument xml, XmlNode xmlNode, out List<XmlElement> disciplines)
		{
			var comps_nodes = xmlNode.ChildNodes;
			List<XmlElement> comps_list = new List<XmlElement>();
			int count = 0;

			if (comps_nodes.Count == 0)
			{
				//output = xml.CreateElement(name);
				//xmlNode.AppendChild(output);
				disciplines = null;
				return false;
			}
			else
			{
				while (count < comps_nodes.Count)
				{
					comps_list.Add(comps_nodes[count] as XmlElement);
					count++;
				}
				disciplines = comps_list;
				//output = comps_node. as XmlElement;
				return true;
			}
		}
	}
}

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