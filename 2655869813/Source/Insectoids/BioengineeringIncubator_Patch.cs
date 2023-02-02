using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using InsectoidBioengineering;
using UnityEngine;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Insectoids
{
    [HarmonyPatch]
    static class BioengineeringIncubator_Patch
    {
        [HarmonyPatch(typeof(Command), nameof(Command.ProcessInput))]
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void BaseCommandProcessInput(Command __instance, Event ev)
        {
            throw new NotImplementedException("stub method should not have been called");
        }

		[HarmonyPatch(typeof(Command_SetFirstGenomeList), nameof(Command_SetFirstGenomeList.ProcessInput))]
		[HarmonyPrefix]
		public static bool First(Command_SetFirstGenomeList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 1);
		}

		[HarmonyPatch(typeof(Command_SetSecondGenomeList), nameof(Command_SetSecondGenomeList.ProcessInput))]
		[HarmonyPrefix]
		public static bool Second(Command_SetSecondGenomeList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 2);
		}

		[HarmonyPatch(typeof(Command_SetThirdGenomeList), nameof(Command_SetThirdGenomeList.ProcessInput))]
		[HarmonyPrefix]
		public static bool Third(Command_SetThirdGenomeList __instance, Event ev)
		{
			return Process(__instance, ev, __instance.building, 3);
		}

		static bool Process(Command command, Event ev, Building_BioengineeringIncubator building, int slot)
		{
			if (!BenLubarsVanillaExpandedPatches.forceItemProcessorStart)
			{
				return true;
			}

			BaseCommandProcessInput(command, ev);

            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("VFEI_None".Translate(), delegate
                {
					switch (slot)
                    {
						case 1:
							building.theFirstGenomeIAmGoingToInsert = "None";
							break;
						case 2:
							building.theSecondGenomeIAmGoingToInsert = "None";
							break;
						case 3:
							building.theThirdGenomeIAmGoingToInsert = "None";
							break;
                    }
                }, extraPartWidth: 29f)
            };

            foreach (var accepted in DefDatabase<AcceptedGenomesDef>.AllDefs)
			{
				foreach (var genome in accepted.genomes.Where(g => g != "None"))
				{
					options.Add(new FloatMenuOption(genome.Translate(), delegate
					{
						switch (slot)
						{
							case 1:
								building.ExpectingFirstGenome = true;
								building.theFirstGenomeIAmGoingToInsert = genome;
								break;
							case 2:
								building.ExpectingSecondGenome = true;
								building.theSecondGenomeIAmGoingToInsert = genome;
								break;
							case 3:
								building.ExpectingThirdGenome = true;
								building.theThirdGenomeIAmGoingToInsert = genome;
								break;
						}
					}, extraPartWidth: 29f));
				}
			}

			Find.WindowStack.Add(new FloatMenu(options));

			return false;
		}
	}
}
