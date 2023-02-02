using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using ItemProcessor;
using RimWorld;
using UnityEngine;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch]
    static class ItemProcessor_Patch
    {
        [HarmonyPatch(typeof(Command), nameof(Command.ProcessInput))]
        [HarmonyReversePatch]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static void BaseCommandProcessInput(Command __instance, Event ev)
        {
            throw new NotImplementedException("stub method should not have been called");
		}

		[HarmonyPatch(typeof(Command_SetFirstItemList), nameof(Command_SetFirstItemList.ProcessInput))]
		[HarmonyPrefix]
		public static bool First(Command_SetFirstItemList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 1);
		}

		[HarmonyPatch(typeof(Command_SetSecondItemList), nameof(Command_SetSecondItemList.ProcessInput))]
		[HarmonyPrefix]
		public static bool Second(Command_SetSecondItemList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 2);
		}

		[HarmonyPatch(typeof(Command_SetThirdItemList), nameof(Command_SetThirdItemList.ProcessInput))]
		[HarmonyPrefix]
		public static bool Third(Command_SetThirdItemList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 3);
		}

		[HarmonyPatch(typeof(Command_SetFourthItemList), nameof(Command_SetFourthItemList.ProcessInput))]
		[HarmonyPrefix]
		public static bool Fourth(Command_SetFourthItemList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 4);
		}

		static bool Process(Command command, Event ev, Building_ItemProcessor building, int slot)
		{
			if (!BenLubarsVanillaExpandedPatches.forceItemProcessorStart || building.compItemProcessor.Props.isCategoryBuilding)
            {
				return true;
            }

            BaseCommandProcessInput(command, ev);

			var options = new List<FloatMenuOption>();
			if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
			{
				options.Add(new FloatMenuOption("IP_None".Translate(), delegate
				{
					SetInputString(building, slot, "None");
				}, extraPartWidth: 29f));
			}

			foreach (var accepted in DefDatabase<ItemAcceptedDef>.AllDefs.Where(a => a.building == building.def.defName && a.slot == slot))
			{
				foreach (var item in accepted.items)
				{
					if (item == "None")
                    {
						continue;
                    }

					options.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
					{
						SetInput(building, slot, item);
					}, extraPartWidth: 29f));
				}
			}

			Find.WindowStack.Add(new FloatMenu(options));

			return false;
        }

		static void SetInputString(Building_ItemProcessor building, int slot, string item)
		{
			switch (slot)
            {
				case 1:
					building.firstItem = item;
					break;
				case 2:
					building.secondItem = item;
					break;
				case 3:
					building.thirdItem = item;
					break;
				case 4:
					building.fourthItem = item;
					break;
            }
		}

		static void SetInput(Building_ItemProcessor building, int slot, string item)
		{
			if (building.processorStage > ProcessorStage.ExpectingIngredients)
			{
				return;
			}

			SetInputString(building, slot, item);

			if (building.compItemProcessor.Props.isSemiAutomaticMachine)
			{
				if (building.compPowerTrader != null && !building.compPowerTrader.PowerOn && building.compItemProcessor.Props.noPowerDestroysProgress)
				{
					Messages.Message("IP_NoPowerDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent);
				}
				else if (building.compFuelable != null && !building.compFuelable.HasFuel && building.compItemProcessor.Props.noPowerDestroysProgress)
				{
					Messages.Message("IP_NoFuelDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent);
				}
				else
				{
					building.IngredientsChosenBringThemIn();
				}
			}
			else
			{
				building.processorStage = ProcessorStage.IngredientsChosen;
			}
		}
	}
}
