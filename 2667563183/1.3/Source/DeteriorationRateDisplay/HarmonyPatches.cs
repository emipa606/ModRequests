using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Noise;
using UnityEngine;

namespace DRD
{
	[HarmonyPatch(typeof(Thing), "GetInspectStringLowPriority")]
    class HarmonyPatches
    {

		static void Postfix(ref Thing __instance, ref string __result)
		{
			float hpLossPerDay = 0;
			float finalDeteriorationRate = SteadyEnvironmentEffects.FinalDeteriorationRate(__instance);
			if (finalDeteriorationRate > .001) //If it's smaller then it takes no damage to leave it at 0.
			{
				var deteriorationRate = Mathf.Lerp(1f, 5f, __instance.Map.weatherManager.RainRate);
				hpLossPerDay = deteriorationRate * finalDeteriorationRate;
				__result += "\nDeterioration rate: " + Math.Round(hpLossPerDay, 2) + " / day";
			}
		}
	}

	/*	The orginal we're patching
	public virtual string GetInspectStringLowPriority()
	{
		string result = null;
		tmpDeteriorationReasons.Clear();
		SteadyEnvironmentEffects.FinalDeteriorationRate(this, tmpDeteriorationReasons);
		if (tmpDeteriorationReasons.Count != 0)
		{
			result = string.Format("{0}: {1}", "DeterioratingBecauseOf".Translate(), tmpDeteriorationReasons.ToCommaList().CapitalizeFirst());
		}
		return result;
	}
*/
		}
