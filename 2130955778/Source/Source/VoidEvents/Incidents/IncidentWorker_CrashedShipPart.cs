﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace VoidEvents
{
	public class IncidentWorker_CrashedShipPart : IncidentWorker
	{
		private const float ShipPointsFactor = 0.9f;

		private const int IncidentMinimumPoints = 300;

		private const float DefendRadius = 28f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (((Map)parms.target).listerThings.ThingsOfDef(def.mechClusterBuilding).Count > 0)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<TargetInfo> list = new List<TargetInfo>();
			ThingDef shipPartDef = def.mechClusterBuilding;
			IntVec3 intVec = FindDropPodLocation(map, (IntVec3 spot) => CanPlaceAt(spot));
			if (intVec == IntVec3.Invalid)
			{
				return false;
			}
			float points = Mathf.Max(parms.points * 0.9f, 300f);
			Thing thing = ThingMaker.MakeThing(shipPartDef);
			GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, thing), intVec, map);
			list.Add(new TargetInfo(intVec, map));
			SendStandardLetter(parms, list);
			return true;

			bool CanPlaceAt(IntVec3 loc)
			{
				CellRect cellRect = GenAdj.OccupiedRect(loc, Rot4.North, shipPartDef.Size);
				if (loc.Fogged(map) || !cellRect.InBounds(map))
				{
					return false;
				}
				if (!DropCellFinder.SkyfallerCanLandAt(loc, map, shipPartDef.Size))
				{
					return false;
				}
				foreach (IntVec3 item2 in cellRect)
				{
					RoofDef roof = item2.GetRoof(map);
					if (roof != null && roof.isNatural)
					{
						return false;
					}
				}
				return GenConstruct.CanBuildOnTerrain(shipPartDef, loc, map, Rot4.North);
			}
		}

		private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
		{
			for (int i = 0; i < 200; i++)
			{
				IntVec3 intVec = RCellFinder.FindSiegePositionFrom_NewTemp(DropCellFinder.FindRaidDropCenterDistant_NewTemp(map, allowRoofed: true), map, allowRoofed: true);
				if (validator(intVec))
				{
					return intVec;
				}
			}
			return IntVec3.Invalid;
		}
	}
}
