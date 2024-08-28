using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Rimatomics;
using RimWorld;
using UnityEngine;
using Verse;

namespace PPC_Plugin
{ 
	[StaticConstructorOnStartup]
    public class PPC_Plugin
	{
		public static Harmony Harmony;
		public static RimatomicResearchDef Project = null;
		public static ThingDef PPCP = null;

		static PPC_Plugin()
        {
			var _this = typeof(Util_Fix);
			Harmony = new Harmony("Yan.PPC_Plugin");

			Harmony.Patch(AccessTools.Constructor(typeof(CompProperties_Battery)), new HarmonyMethod(_this, "CompProperties_Battery"));
			Harmony.Patch(AccessTools.Method("Rimatomics.PPC_Util:DissipateCharge"), new HarmonyMethod(_this, "DissipateCharge"));
            Harmony.Patch(AccessTools.Method("Rimatomics.PPC_Util:HasCharge"), new HarmonyMethod(_this, "HasCharge"));
			Log.Message("[PPCP] Patched Rimatomics");

			LongEventHandler.QueueLongEvent(PPCP_Fix, "PPC Plugin Fix" , true, null);
		}

		public static void PPCP_Fix()
        {
			Log.Message("[PPCP] PPCP Loadding");
			Project = DefDatabase<RimatomicResearchDef>.GetNamed("Research_PPC_Plugin");
			PPCP = DefDatabase<ThingDef>.GetNamed("PPC_Plugin");
			var add_comp = new CompProperties_Upgradable()
			{
				upgrades = {
						new Upgrades() {
							project = Project
						}
					}
			};
			try
            {
				foreach (var x in DefDatabase<ThingDef>.AllDefs)
				{
					var flag = false;
					foreach (var i in x.comps)
                    {
						if (i.GetType() == typeof(CompProperties_Battery))
						{
							i.compClass = typeof(CompPPC);
							flag = true;
						}
					}
					if (x.defName == "PPC")
						continue;
					if (!flag)
						continue;
					x.comps.Add(add_comp);
				};
			}
			finally
			{ 
				Log.Message("[PPCP] PPCP Loadded");
			}
		}
	}

	[StaticConstructorOnStartup]
	public class PPC_MC : UniversalPipeMapComp
    {
		public static Dictionary<int, PPC_MC> CompCache = new Dictionary<int, PPC_MC>();
		public static PPC_MC loccachecomp = null;

		public List<CompPPC> CPBs = new List<CompPPC>();

		public PPC_MC(Map map) : base(map)
		{
			if (CompCache.ContainsKey(base.map.uniqueID))
				CompCache[base.map.uniqueID] = this;
			else
				CompCache.Add(base.map.uniqueID, this);
			loccachecomp = null;
		}

		public override void MapRemoved()
		{
			base.MapRemoved();
			CompCache.Remove(map.uniqueID);
			loccachecomp = null;
		}

	}


	public static class Util_Fix
    {
		public static PPC_MC PPCPlugin(this Map map)
		{
			if (PPC_MC.loccachecomp != null && PPC_MC.loccachecomp.map.uniqueID == map.uniqueID)
				return PPC_MC.loccachecomp;
			PPC_MC.loccachecomp = PPC_MC.CompCache[map.uniqueID];
			return PPC_MC.loccachecomp;
		}

		public static bool HasCharge(ref bool __result, PowerNet PowerNet, float charge)
		{
			List<CompPowerBattery> list = (from x in PowerNet.Map.Rimatomics().PPCs
										   where x.PowerComp.PowerNet == PowerNet
										   select x.batt into x
										   where x.StoredEnergy > 0f
										   select x).ToList();
			list.AddRange(from x in PowerNet.Map.PPCPlugin().CPBs
						  where x.PowerNet == PowerNet && x.StoredEnergy > 0f && x.parent.GetComp<CompUpgradable>().HasUpgrade(PPC_Plugin.PPCP)
						  select x);
			if (list.NullOrEmpty())
            {
				__result = false;
				return false;
			}
			if (list.Sum((CompPowerBattery x) => x.StoredEnergy) > charge)
            {
				__result = true;
				return false;
			}
			__result = false;
			return false;
		}
		public static bool DissipateCharge(ref bool __result, PowerNet PowerNet, float charge)
		{
			Log.Warning("PPC Plugin DissipateCharge");
			List<CompPowerBattery> list = (from x in PowerNet.Map.Rimatomics().PPCs
										   where x.PowerComp.PowerNet == PowerNet
										   select x.batt into x
										   where x.StoredEnergy > 0f
										   select x).ToList();
			list.AddRange(from x in PowerNet.Map.PPCPlugin().CPBs
						  where x.PowerNet == PowerNet && x.StoredEnergy > 0f && x.parent.GetComp<CompUpgradable>().HasUpgrade(PPC_Plugin.PPCP)
						  select x);

			if (list.NullOrEmpty())
			{
				__result = false;
				return false;
			}
			if (list.Sum((CompPowerBattery x) => x.StoredEnergy) < charge)
			{
				__result = false;
				return false;
			}
			
			int i = 0;
			while (charge > 0f)
			{
				i++;
				list.RemoveAll((CompPowerBattery x) => x.StoredEnergy <= 0f);
				if (list.NullOrEmpty() || list.Sum((CompPowerBattery x) => x.StoredEnergy) < charge)
                {
					__result = false;
					return false;
				}
				float power = Mathf.Min(charge / list.Count, list.Min((CompPowerBattery x) => x.StoredEnergy));
				foreach (CompPowerBattery item in list)
				{
					item.DrawPower(power);
					charge -= power;
				}
				if (i > 5000)
                {
					__result = false;
					return false;
				}
			}
			if (DebugSettings.godMode)
				Log.Warning(i.ToString() + "Pulse loops (god mode on)");
			__result = true;
			return false;
		}
		public static bool CompProperties_Battery(ref CompProperties_Battery __instance)
        {
			__instance.compClass = typeof(CompPPC);
			return false;
        }
	}

	public class CompPPC : CompPowerBattery
	{
		public bool is_PPC = false;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref is_PPC, "is_PPC", false);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			is_PPC = parent.def.defName == "PPC";
			if (is_PPC)
				return;
			if (!parent.Map.PPCPlugin().CPBs.Contains(this))
				parent.Map.PPCPlugin().CPBs.Add(this);
			if (!parent.Map.Rimatomics().Upgradables.Contains(parent))
				parent.Map.Rimatomics().Upgradables.Add(parent);
		}
		public override void PostDeSpawn(Map map)
		{
			if (!is_PPC)
			{
				if (map.Rimatomics().Upgradables.Contains(parent))
					map.Rimatomics().Upgradables.Remove(parent);
				if (map.PPCPlugin().CPBs.Contains(this))
					map.PPCPlugin().CPBs.Remove(this);
			}
			base.PostDeSpawn(map);
		}
	}
}
