using RimWorld;
using Verse;
using UnityEngine;
using HugsLib;
using HugsLib.Settings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace CustomStack
{
	public class CustomStackHL : ModBase
	{
		SettingHandle<bool> CustomStackRestart;
		Dictionary<ThingDef, int> CustomStackRef = new Dictionary<ThingDef, int>();
		Dictionary<ThingDef, SettingHandle<int>> CustomStackList = new Dictionary<ThingDef, SettingHandle<int>>();

		public override string ModIdentifier
		{
			get
			{
				return "CustomStack";
			}
		}

		public override void DefsLoaded()
		{
			CustomStackRestart = Settings.GetHandle<bool>("restart", "CustomStack_restart_name".Translate(), "CustomStack_restart_tip".Translate(), false);
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDef.stackLimit > 1)
				{
					CustomStackRef.Add(thingDef, thingDef.stackLimit);
					CustomStackList.Add(thingDef, Settings.GetHandle<int>(thingDef.defName, (thingDef.defName).Translate(), "", thingDef.stackLimit));
				}
				if (CustomStackRestart.Value)
				{
					if (thingDef.stackLimit == 1)
                    {
						CustomStackRef.Add(thingDef, thingDef.stackLimit);
						CustomStackList.Add(thingDef, Settings.GetHandle<int>(thingDef.defName, (thingDef.defName).Translate(), "", thingDef.stackLimit));
					}
				}
			}
			ChangeXML();
			Log.Message("CustomStack loaded : " + CustomStackRef.Keys.Count + " values found");
			base.DefsLoaded();
		}

		public override void SettingsChanged()
		{
			ChangeXML();
			Log.Message("CustomStack's settings updated");
			base.SettingsChanged();
		}

		public void ChangeXML()
		{
			foreach (KeyValuePair<ThingDef, SettingHandle<int>> item in CustomStackList)
			{
				item.Key.stackLimit = item.Value;
			}
		}
	}
}