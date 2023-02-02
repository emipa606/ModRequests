using RimWorld;
using System.Collections.Generic;
using Verse;
using static RimWorld.GenLabel;

namespace RenameGun
{
    public static class GenLabelFixed
	{
		public static string ThingLabel(string name, Thing t, int stackCount, bool includeStuff, bool includeQuality, bool includeHP)
		{
			LabelRequest key = default(LabelRequest);
			key.entDef = t.def;
			key.styleDef = t.StyleDef;
			key.stackCount = stackCount;
			key.stuffDef = includeStuff ? t.Stuff : null;
			key.hasQuality = includeQuality ? t.TryGetQuality(out key.quality) : false;
			if (t.def.useHitPoints && includeHP)
			{
				key.health = t.HitPoints;
				key.maxHealth = t.MaxHitPoints;
			}
			Apparel apparel = t as Apparel;
			if (apparel != null)
			{
				key.wornByCorpse = apparel.WornByCorpse;
			}
			return NewThingLabel(name, t, stackCount, includeStuff, includeQuality, includeHP);
		}

		public static string ThingLabel(string fixedName, BuildableDef entDef, ThingDef stuffDef, int stackCount = 1)
		{
			LabelRequest key = default(LabelRequest);
			key.entDef = entDef;
			key.stuffDef = stuffDef;
			key.stackCount = stackCount;
			return NewThingLabel(fixedName, stuffDef, stackCount);
		}
		private static string NewThingLabel(string fixedName, ThingDef stuffDef, int stackCount)
		{
			string text = ((stuffDef != null) ? ((string)"ThingMadeOfStuffLabel".Translate(stuffDef.LabelAsStuff, fixedName)) : fixedName);
			if (stackCount > 1)
			{
				text = text + " x" + stackCount.ToStringCached();
			}
			return text;
		}

		private static string NewThingLabel(string name, Thing t, int stackCount, bool includeStuff, bool includeQuality, bool includeHp)
		{
			ThingStyleDef styleDef = t.StyleDef;
			string text = ((styleDef == null || styleDef.overrideLabel.NullOrEmpty()) ? 
				ThingLabel(name, t.def, includeStuff ? t.Stuff : null) : styleDef.overrideLabel);
			QualityCategory qc;
			bool flag = t.TryGetQuality(out qc) && includeQuality;
			int hitPoints = t.HitPoints;
			int maxHitPoints = t.MaxHitPoints;
			bool flag2 = t.def.useHitPoints && hitPoints < maxHitPoints && t.def.stackLimit == 1 && includeHp;
			bool flag3 = (t as Apparel)?.WornByCorpse ?? false;
			if (flag || flag2 || flag3)
			{
				text += " (";
				if (flag)
				{
					text += qc.GetLabel();
				}
				if (flag2)
				{
					if (flag)
					{
						text += " ";
					}
					text += ((float)hitPoints / (float)maxHitPoints).ToStringPercent();
				}
				if (flag3)
				{
					if (flag || flag2)
					{
						text += " ";
					}
					text += "WornByCorpseChar".Translate();
				}
				text += ")";
			}
			if (stackCount > 1)
			{
				text = text + " x" + stackCount.ToStringCached();
			}
			return text;
		}
	}
}
