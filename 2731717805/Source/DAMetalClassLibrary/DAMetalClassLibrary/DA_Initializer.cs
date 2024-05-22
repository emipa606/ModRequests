using System;
using UnityEngine;
using Verse;

namespace dragonagemetals
{
	[StaticConstructorOnStartup]
	internal static class DA_Initializer
	{
		static DA_Initializer()
		{
			LongEventHandler.QueueLongEvent(new Action(DA_Initializer.Setup), "LibraryStartup", false, null, true);
		}
		public static void Setup()
		{
			ThingDef thingDef = ThingDef.Named("Veridium");
			thingDef.stuffProps.commonality = DAMetalMod.Settings.veridiumCommonality;
			thingDef.deepCommonality = DAMetalMod.Settings.veridiumDeepCommonality;
			ThingDef.Named("MineableVeridium").building.mineableScatterCommonality = DAMetalMod.Settings.veridiumMineableCommonality;

			ThingDef thingDef2 = ThingDef.Named("SilveriteOre");
			thingDef2.stuffProps.commonality = DAMetalMod.Settings.SilveriteOreCommonality;
			thingDef2.deepCommonality = DAMetalMod.Settings.SilveriteOreDeepCommonality;
			ThingDef.Named("MineableSilverite").building.mineableScatterCommonality = DAMetalMod.Settings.SilveriteMineableCommonality;

			ThingDef thingDef3 = ThingDef.Named("VolcanicAurum");
			thingDef3.stuffProps.commonality = DAMetalMod.Settings.volcanicAurumCommonality;
			thingDef3.deepCommonality = DAMetalMod.Settings.volcanicAurumDeepCommonality;
			ThingDef.Named("MineableVolcanicAurum").building.mineableScatterCommonality = DAMetalMod.Settings.volcanicAurumMineableCommonality;

			ThingDef thingDef4 = ThingDef.Named("Lyrium");
			thingDef4.stuffProps.commonality = DAMetalMod.Settings.lyriumCommonality;
			thingDef4.deepCommonality = DAMetalMod.Settings.lyriumDeepCommonality;
			ThingDef.Named("MineableLyrium").building.mineableScatterCommonality = DAMetalMod.Settings.lyriumMineableCommonality;

			ThingDef thingDef5 = ThingDef.Named("FoldedLyrium");
			thingDef5.stuffProps.commonality = DAMetalMod.Settings.foldedLyriumCommonality;
			
			ThingDef thingDef6 = ThingDef.Named("MeteoriteOre");
			thingDef6.stuffProps.commonality = DAMetalMod.Settings.meteoriteOreCommonality;
			
		}
	}
}
