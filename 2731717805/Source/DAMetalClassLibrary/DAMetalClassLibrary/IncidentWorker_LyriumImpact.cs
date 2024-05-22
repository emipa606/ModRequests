using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace dragonagemetals
{
	public class IncidentWorker_LyriumImpact : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return this.TryFindCell(out intVec, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!this.TryFindCell(out intVec, map))
			{
				return false;
			}
			List<Thing> list = DefDatabase<ThingSetMakerDef>.GetNamed("DA_Lyrium", true).root.Generate();
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, intVec, map);
			LetterDef baseLetterDef = list[0].def.building.isResourceRock ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent;
			string str = string.Format(this.def.letterText, list[0].def.label).CapitalizeFirst();
			base.SendStandardLetter(this.def.letterLabel + ": " + list[0].def.LabelCap, str, baseLetterDef, parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
			return true;
		}

		private bool TryFindCell(out IntVec3 cell, Map map)
		{
			int maxMineables = ThingSetMaker_Lyrium.MineablesCountRange.max;
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, true, true, delegate (IntVec3 x)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
				CellRect cellRect = CellRect.CenteredOn(x, num, num);
				int num2 = 0;
				foreach (IntVec3 c in cellRect)
				{
					if (c.InBounds(map) && c.Standable(map))
					{
						num2++;
					}
				}
				return num2 >= maxMineables;
			});
		}
	}
}
