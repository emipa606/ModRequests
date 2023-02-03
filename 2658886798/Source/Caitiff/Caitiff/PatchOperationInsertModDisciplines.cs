using System;
using System.Collections;
using System.Xml;
using System.Linq;
using Verse;

namespace Vampire
{
	public class PatchOperationInsertModDisciplines : PatchOperation
	{
	#pragma warning disable 649
		private string modName;
		private string afterDiscipline;
	#pragma warning restore 649

		protected override bool ApplyWorker(XmlDocument xml)
		{
			// Simplify test - Just figure out how to add the discipline, and work from there!
			//string xPath = xml.GetXPath();
			string xPath = xml.GetXPath();
			XmlNode xNode = xml.SelectSingleNode(xPath);
			XmlNode xAfter = xNode.SelectSingleNode(afterDiscipline);
			bool result = false;
			if (modName == "Rim of Madness - Clan Malkavian" && ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName))
			{
				xNode.InsertAfter(xml.CreateElement("<li>ROMV_Dementation</li>"), xAfter);
				result = true;
			}
			return result;
		}
	}
}
