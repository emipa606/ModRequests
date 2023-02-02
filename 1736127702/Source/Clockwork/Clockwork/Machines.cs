using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Clockwork
{
	public class ThingComp_ClockworkMachine : ThingComp
	{
		public CompProperties_ClockworkMachine Props => (CompProperties_ClockworkMachine)props;
		private float assemblyProgress;

		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;
		private bool SteamOn => parent.GetComp<ThingComp_SteamConsumer>()?.running ?? false;

		private List<IntVec3> cachedAdjCellsCardinal;


		public override void CompTick()
		{
			if (!parent.Spawned)
			{
				return;
			}
			CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
			if (comp != null)
			{
				if (!comp.Awake)
				{
					return;
				}
			}
			else if (parent.Position.Fogged(parent.Map))
			{
				return;
			}
			if ((!Props.requiresPower || PowerOn) || (SteamOn))
			{
				if (Props.machineInput == null)
                {
					float num = 1f / (Props.hoursToAssembly * 2500f);
					assemblyProgress += num;
					if (assemblyProgress > 1f)
					{
						TryOutputNoInput();
						ResetCountdown();
					}
				}
				else if (HasEnoughMaterialInHoppers() && HasOutput())
				{
					float num = 1f / (Props.hoursToAssembly * 2500f);
					assemblyProgress += num;
					CheckShouldSpawn();
				}

			}

		}

		private void CheckShouldSpawn()
		{
			if (assemblyProgress > 1f)
			{
				TryOutput();
				ResetCountdown();
			}
		}
		private void ResetCountdown()
		{
			assemblyProgress = 0f;
		}
		public override string CompInspectStringExtra()
		{
			if (!HasOutput())
			{
				return "MachineNeedsOutput".Translate();

			}
			else if (Props.machineInput != null && !HasEnoughMaterialInHoppers())
			{
				return "MachineNeedsMaterials".Translate() + ": " + Props.machineInput.label + " x" + Props.machineMaterialCost;

			}
			return "AssemblyProgress".Translate() + ": " + assemblyProgress.ToStringPercent();
		}
		public List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (cachedAdjCellsCardinal == null)
				{
					cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(parent)
											  where c.InBounds(parent.Map)
											  select c).ToList();
				}
				return cachedAdjCellsCardinal;
			}
		}

		public bool TryOutput()
		{
			if (!HasEnoughMaterialInHoppers() || !(assemblyProgress > 1f))
			{
				return false;
			}
			List<ThingDef> list = new List<ThingDef>();

			Thing thing = FindMaterialInAnyHopper();

			if (thing == null)
			{
				Log.Error("Did not find enough materials in hoppers.");
				return false;
			}
			int num = Props.machineMaterialCost;
			list.Add(thing.def);
			thing.SplitOff(num);

			if (TryFindOutput(parent, Props.machineOutput, Props.machineOutputAmount, out IntVec3 result))
			{
				Thing thing2 = ThingMaker.MakeThing(Props.machineOutput);
				thing2.stackCount = Props.machineOutputAmount;
				GenPlace.TryPlaceThing(thing2, result, parent.Map, ThingPlaceMode.Direct, out Thing lastResultingThing);
				return true;
			}
			return false;

		}

		public bool TryOutputNoInput()
		{
			if (!(assemblyProgress > 1f))
			{
				return false;
			}

			if (TryFindOutput(parent, Props.machineOutput, Props.machineOutputAmount, out IntVec3 result))
			{
				Thing thing2 = ThingMaker.MakeThing(Props.machineOutput);
				thing2.stackCount = Props.machineOutputAmount;
				GenPlace.TryPlaceThing(thing2, result, parent.Map, ThingPlaceMode.Direct, out Thing lastResultingThing);
				return true;
			}
			return false;

		}

		public virtual Thing FindMaterialInAnyHopper()
		{
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = AdjCellsCardinalInBounds[i].GetThingList(parent.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (thing3.def == Props.machineInput)
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.ClockworkInput)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					return thing;
				}
			}
			return null;
		}
		public virtual bool HasEnoughMaterialInHoppers()
		{
			int num = 0;
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = AdjCellsCardinalInBounds[i];
				Thing thing = null;
				Thing thing2 = null;
				List<Thing> thingList = c.GetThingList(parent.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
					if (thing3.def == Props.machineInput)
					{
						thing = thing3;
					}
					if (thing3.def == ThingDefOf.ClockworkInput)
					{
						thing2 = thing3;
					}
				}
				if (thing != null && thing2 != null)
				{
					num += thing.stackCount;
				}
				if (num >= Props.machineMaterialCost)
				{
					return true;
				}
			}
			return false;
		}
		public virtual bool HasOutput()
		{
			for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = AdjCellsCardinalInBounds[i];
				List<Thing> thingList = c.GetThingList(parent.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (thing.def == ThingDefOf.ClockworkOutput)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static bool TryFindOutput(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent))
			{
				List<Thing> thingList = item.GetThingList(parent.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (thing.def == ThingDefOf.ClockworkOutput)
					{
						result = item;
						return true;
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}
	}

	public class CompProperties_ClockworkMachine : CompProperties
	{
		public ThingDef machineInput;
		public ThingDef machineOutput;
		public int machineMaterialCost = 1;
		public int machineOutputAmount = 1;
		public float hoursToAssembly = 1f;
		public bool requiresPower = true;

		public CompProperties_ClockworkMachine()
		{
			compClass = typeof(ThingComp_ClockworkMachine);
		}
	}
}