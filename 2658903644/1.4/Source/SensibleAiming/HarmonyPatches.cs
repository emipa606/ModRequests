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

namespace SA
{
	[HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_IgnoringPosture", MethodType.Getter)]
    class HarmonyPatches
    {
		static void Postfix(ref float __result)
		{
			__result *= 2;
		}
	}
}
