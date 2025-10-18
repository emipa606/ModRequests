using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace Umbrellas {
	[HarmonyPatch(typeof(ApparelUtility), "HasPartsToWear")]
	class RBCanWearPatch {
		static List<string> prosthetics = new List<string>() {"SimpleProstheticArm", "BionicArm", "ArchotechArm", "AdvancedPowerArm", "AdvancedBionicArm", "SteelArm", "PowerArm", "Arm", "SNS_Hediff_BionicArm_GenI", "SNS_Hediff_BionicArm_GenII", "SNS_Hediff_BionicArm_GenIII", "SNS_Hediff_BionicArm_GenIV" };
		static void Postfix (Pawn p, ThingDef apparel, ref bool __result) {
			if (__result == false && UmbrellaDefMethods.UmbrellaDefs.Contains(apparel)) {
				foreach (Hediff hediff in p.health.hediffSet.hediffs) {
					if (prosthetics.Contains(hediff.def.defName)) {
						__result = true;
                    }
                }
            }
        }
	}
}
